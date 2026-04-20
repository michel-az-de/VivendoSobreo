import fitz, sys, re
sys.stdout.reconfigure(encoding='utf-8')

pdf_path = r'C:\Users\f.michel.de.azevedo\Downloads\08 - Livros - AA e Espiritismo\Alcoolicos-Anonimos-Livro-Azul.pdf'
doc = fitz.open(pdf_path)

# Check pages around each chapter to find the titles manually
chapter_pages = {1: 35, 2: 65, 3: 88, 4: 113, 5: 139, 6: 165, 7: 197, 8: 224, 9: 258, 10: 284, 11: 310}

for cap_num, page_idx in sorted(chapter_pages.items()):
    print(f"\n=== CAPÍTULO {cap_num} (pages {page_idx+1} to {page_idx+3}) ===")
    for check_page in range(page_idx, min(page_idx+4, doc.page_count)):
        text = doc[check_page].get_text()
        text = re.sub(r'Página \d+ de \d+\s*\n?', '', text)
        lines = [l.strip() for l in text.split('\n') if l.strip()]
        print(f"  -- page {check_page+1} --")
        for j, line in enumerate(lines[:10]):
            print(f"    [{j}] |{line}|")
