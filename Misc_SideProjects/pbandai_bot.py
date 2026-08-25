import tkinter as tk
from tkinter import messagebox
from playwright.sync_api import sync_playwright
import time
import msvcrt


def start_bot(product_url, refresh_interval):
    def run(playwright):
        user_data_dir = "./user_data"
        
        print(f"Launching browser for: {product_url}")
        print(f"Refresh interval set to: {refresh_interval} seconds")
        
        browser = playwright.chromium.launch_persistent_context(
            user_data_dir=user_data_dir,
            headless=False, 
            args=[
                "--start-maximized", 
                "--disable-blink-features=AutomationControlled"
            ],
            ignore_default_args=["--enable-automation"],
            user_agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            no_viewport=True
        )
        
        page = browser.pages[0]
        page.goto(product_url)
        
        print("\n--- IMPORTANT ---")
        print("1. Please log in to your Premium Bandai account in the newly opened window.")
        print("2. Make sure you don't close the browser!")
        print("3. When you are ready on the product page, come back to this terminal and press ENTER.")
        input("Press ENTER to start the auto-refresh loop...\n")

        print("Bot started! Refreshing page looking for the pre-order button...")
        print("💡 TIP: Press 'p' in this terminal at any time to PAUSE, and 'r' to RESUME.\n")
        
        while True:
            # Check for key presses
            if msvcrt.kbhit():
                key = msvcrt.getch().decode('utf-8', 'ignore').lower()
                if key == 'p':
                    print("\n⏸️ BOT PAUSED! Press 'r' to resume...")
                    while True:
                        if msvcrt.kbhit() and msvcrt.getch().decode('utf-8', 'ignore').lower() == 'r':
                            print("▶️ RESUMING BOT...\n")
                            break
                        time.sleep(0.1)

            try:
                # Check if the page was accidentally closed, if so, reopen it
                if len(browser.pages) == 0:
                    print("Browser was closed! Re-opening...")
                    page = browser.new_page()
                    page.goto(product_url)
                elif page.is_closed():
                    print("Tab was closed! Switching back...")
                    page = browser.pages[-1]
                    if product_url not in page.url:
                        page.goto(product_url)
                        
                import re
                
                # Look for buttons or elements with 'btn' class containing checkout keywords
                preorder_button = page.locator("button, .btn, .cart-btn").filter(
                    has_text=re.compile(r"PRE-ORDER|PLACE PRE-ORDER|ADD TO CART|CART", re.IGNORECASE)
                )
                
                if preorder_button.count() > 0:
                    btn = preorder_button.first
                    if btn.is_visible() and btn.is_enabled():
                        print(f"BUTTON FOUND! ({btn.inner_text().strip()}) Clicking it now...")
                        btn.click()
                        
                        print("\nSUCCESS: Clicked the button! Please finish the checkout process in the browser manually now.")
                        break
                    
            except Exception as e:
                pass
                
            print(f"Not available yet. Refreshing in {refresh_interval} seconds...")
            time.sleep(refresh_interval)
            
            try:
                if not page.is_closed():
                    page.reload()
                    page.wait_for_load_state("domcontentloaded")
            except Exception as e:
                print("Encountered an error while refreshing, but keeping the bot alive...")

        # Keep browser open after clicking so you can checkout
        page.wait_for_timeout(600000) 

    with sync_playwright() as playwright:
        run(playwright)


def start_gui():
    root = tk.Tk()
    root.title("Pre-Order Bot Setup")
    root.geometry("450x250")
    
    # URL Input
    tk.Label(root, text="Product URL:", font=("Arial", 10, "bold")).pack(pady=(15, 5))
    url_entry = tk.Entry(root, width=60)
    url_entry.insert(0, "https://p-bandai.com/sg/item/A2866726001")
    url_entry.pack(pady=5, padx=10)
    
    # Interval Input
    tk.Label(root, text="Refresh Interval (seconds):", font=("Arial", 10, "bold")).pack(pady=(15, 5))
    interval_entry = tk.Entry(root, width=15)
    interval_entry.insert(0, "10.0")
    interval_entry.pack(pady=5)
    
    def on_start():
        url = url_entry.get().strip()
        try:
            interval = float(interval_entry.get().strip())
        except ValueError:
            messagebox.showerror("Invalid Input", "The refresh interval must be a valid number (e.g. 10.0 or 5).")
            return
            
        # Close the GUI window and start the bot
        root.destroy()
        start_bot(url, interval)
        
    tk.Button(root, text="🚀 Launch Bot", command=on_start, bg="#28a745", fg="white", font=("Arial", 12, "bold"), padx=10, pady=5).pack(pady=20)
    
    root.mainloop()

if __name__ == "__main__":
    start_gui()
