import os
import glob
import numpy as np
import tensorflow as tf
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.applications import MobileNetV2
from tensorflow.keras.layers import Dense, GlobalAveragePooling2D, Dropout
from tensorflow.keras.models import Model
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.preprocessing.image import load_img, img_to_array
from sklearn.model_selection import train_test_split

print("TensorFlow version:", tf.__version__)

# 1. Configuration
# (If running on Colab after unzipping, the path will be /content/PM Sensor Reading)
DATASET_DIR = "/content/PM Sensor Reading"
IMG_SIZE = (224, 224)
BATCH_SIZE = 16
EPOCHS = 10

def load_dataset(data_dir):
    print("Scanning dataset directory...")
    image_paths = []
    labels = []
    
    # Recursively find all JPG images
    all_files = glob.glob(os.path.join(data_dir, "**", "*.jpg"), recursive=True)
    all_files += glob.glob(os.path.join(data_dir, "**", "*.JPG"), recursive=True)
    
    for file_path in all_files:
        filename = os.path.basename(file_path).lower()
        if filename.startswith("positive_"):
            image_paths.append(file_path)
            labels.append(1)  # Positive = 1
        elif filename.startswith("negative_"):
            image_paths.append(file_path)
            labels.append(0)  # Negative = 0
            
    return image_paths, labels

# 2. Load and Prepare Data
image_paths, labels = load_dataset(DATASET_DIR)
print(f"Found {len(image_paths)} valid images ({labels.count(1)} Positive, {labels.count(0)} Negative).")

if len(image_paths) == 0:
    print("Error: No images found! Please check the DATASET_DIR path.")
    exit(1)

# Split into Training (80%) and Validation (20%)
X_train_paths, X_val_paths, y_train, y_val = train_test_split(
    image_paths, labels, test_size=0.2, random_state=42, stratify=labels
)

# Custom data generator because we are loading from file paths directly
def data_generator(paths, labels, batch_size, is_training=False):
    num_samples = len(paths)
    
    # Data Augmentation (Only for training)
    datagen = ImageDataGenerator(
        rotation_range=15,
        width_shift_range=0.1,
        height_shift_range=0.1,
        zoom_range=0.1,
        horizontal_flip=True,
        brightness_range=[0.8, 1.2]
    ) if is_training else ImageDataGenerator()

    while True:
        # Shuffle at the start of each epoch
        if is_training:
            combined = list(zip(paths, labels))
            np.random.shuffle(combined)
            paths, labels = zip(*combined)
            
        for offset in range(0, num_samples, batch_size):
            batch_paths = paths[offset:offset+batch_size]
            batch_labels = labels[offset:offset+batch_size]
            
            X_batch = []
            for p in batch_paths:
                img = load_img(p, target_size=IMG_SIZE)
                x = img_to_array(img)
                x = tf.keras.applications.mobilenet_v2.preprocess_input(x)
                X_batch.append(x)
                
            X_batch = np.array(X_batch)
            y_batch = np.array(batch_labels)
            
            if is_training:
                # Apply data augmentation on the fly
                X_batch, y_batch = next(datagen.flow(X_batch, y_batch, batch_size=len(X_batch), shuffle=False))
                
            yield X_batch, y_batch

# 3. Build the Transfer Learning Model
print("Downloading MobileNetV2 base model...")
base_model = MobileNetV2(weights='imagenet', include_top=False, input_shape=(IMG_SIZE[0], IMG_SIZE[1], 3))

# Freeze the base model layers
base_model.trainable = False

# Add a custom classification head on top
x = base_model.output
x = GlobalAveragePooling2D()(x)
x = Dropout(0.2)(x)
predictions = Dense(1, activation='sigmoid')(x) # Binary classification (0 or 1)

model = Model(inputs=base_model.input, outputs=predictions)

model.compile(optimizer=Adam(learning_rate=0.001), 
              loss='binary_crossentropy', 
              metrics=['accuracy'])

# 4. Train the Model
print("Starting training...")
train_gen = data_generator(X_train_paths, y_train, BATCH_SIZE, is_training=True)
val_gen = data_generator(X_val_paths, y_val, BATCH_SIZE, is_training=False)

train_steps = len(X_train_paths) // BATCH_SIZE
val_steps = len(X_val_paths) // BATCH_SIZE

history = model.fit(
    train_gen,
    steps_per_epoch=train_steps,
    validation_data=val_gen,
    validation_steps=val_steps,
    epochs=EPOCHS
)

# 5. Save the final model
model.save("oht_sensor_model.h5")
print("Model saved to oht_sensor_model.h5!")
