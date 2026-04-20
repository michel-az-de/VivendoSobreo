import fitz, json, re, sys
sys.stdout.reconfigure(encoding='utf-8')

pdf_path = r'C:\Users\f.michel.de.azevedo\Downloads\08 - Livros - AA e Espiritismo\Alcoolicos-Anonimos-Livro-Azul.pdf'
doc = fitz.open(pdf_path)

# Hardcoded chapter boundaries (0-indexed pages) and titles
# Start page is the first page of content (not the CAPITULO marker itself)
chapters = [
    (1,  "A História de Bill",             36,  65),
    (2,  "Há Uma Solução",                 65,  88),
    (3,  "Mais Sobre o Alcoolismo",        88, 113),
    (4,  "Nós, os Agnósticos",            113, 139),
    (5,  "Como Funciona",                 139, 165),
    (6,  "Em Ação",                       165, 197),
    (7,  "Trabalhando com os Outros",     197, 224),
    (8,  "Às Esposas",                    224, 258),
    (9,  "A Família Depois",              258, 284),
    (10, "Aos Empregadores",              284, 310),
    (11, "Uma Visão para Você",           310, 377),
]

def extract_chapter(start_page, end_page):
    raw = ""
    for i in range(start_page, end_page):
        text = doc[i].get_text()
        text = re.sub(r'Página \d+ de \d+\s*\n?', '', text)
        raw += text
    return raw

def clean_text(raw):
    # Remove chapter markers
    raw = re.sub(r'CAPÍTULO\s+\d+\s*\n?', '', raw)
    # Remove known all-caps titles that are headers
    cap_titles = ['A HISTÓRIA DE BILL', 'HÁ UMA SOLUÇÃO', 'MAIS SOBRE O ALCOOLISMO',
                  'NÓS, OS AGNÓSTICOS', 'COMO FUNCIONA', 'EM AÇÃO',
                  'TRABALHANDO COM OS OUTROS', 'ÀS ESPOSAS', 'A FAMÍLIA DEPOIS',
                  'AOS EMPREGADORES', 'UMA VISÃO PARA VOCÊ']
    for title in cap_titles:
        raw = raw.replace(title + '\n', '').replace(title, '')

    lines = raw.split('\n')
    clean_lines = []
    for line in lines:
        stripped = line.strip()
        if len(stripped) <= 1:
            continue
        clean_lines.append(stripped)

    # Join into paragraphs
    paragraphs = []
    current = []
    for line in clean_lines:
        if not line:
            if current:
                paragraphs.append(' '.join(current))
                current = []
        else:
            current.append(line)
    if current:
        paragraphs.append(' '.join(current))

    return '\n\n'.join(p for p in paragraphs if p)

def make_short(full_text, max_chars=250):
    first_para = full_text.split('\n\n')[0].replace('\n', ' ')
    if len(first_para) <= max_chars:
        return first_para
    return first_para[:max_chars].rsplit(' ', 1)[0] + '...'

textos = []
for num, title, pg_start, pg_end in chapters:
    raw = extract_chapter(pg_start, pg_end)
    full = clean_text(raw)
    short = make_short(full)

    textos.append({
        "id": num,
        "titulo": title,
        "texto_resumido": short,
        "texto_completo": full
    })
    print(f"Cap {num:02d}: {title} ({len(full)} chars) | Short: {short[:60]}...")

# Load existing literaturas.json and append
with open('src/SoPorHoje.App/Resources/Raw/literaturas.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Check if livro azul already exists
existing_ids = [l['id'] for l in data['livros']]
if 'livro_azul' in existing_ids:
    # Update
    for i, l in enumerate(data['livros']):
        if l['id'] == 'livro_azul':
            data['livros'][i]['textos'] = textos
            print("\nUpdated existing livro_azul entry.")
else:
    data['livros'].append({
        "id": "livro_azul",
        "titulo": "Alcoólicos Anônimos",
        "autor": "Alcoólicos Anônimos",
        "descricao": "O texto básico de A.A., conhecido como o Livro Azul.",
        "textos": textos
    })
    print("\nAdded new livro_azul entry.")

# Increment versao
data['versao'] = data.get('versao', 1) + 1

with open('src/SoPorHoje.App/Resources/Raw/literaturas.json', 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)

total = sum(len(l['textos']) for l in data['livros'])
print(f"\nTotal livros: {len(data['livros'])}, Total textos: {total}")
print(f"JSON versao: {data['versao']}")
print("Saved!")
doc.close()
