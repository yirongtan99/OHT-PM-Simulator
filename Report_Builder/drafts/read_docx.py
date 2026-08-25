from docx import Document

doc = Document(r'C:\Users\yiron\Desktop\FYProject\Report_Builder\drafts\Presentation Report in Markdown.docx')

with open(r'C:\Users\yiron\Desktop\FYProject\Report_Builder\drafts\pres_report_extracted.txt', 'w', encoding='utf-8') as f:
    for para in doc.paragraphs:
        if para.text.strip():
            f.write(para.text + '\n')
