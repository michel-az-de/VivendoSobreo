import json, sys
sys.stdout.reconfigure(encoding='utf-8')

# Load new data
with open(r'C:\Users\f.michel.de.azevedo\Downloads\intergrupos-aa-reunioes-2026-04-18.json', 'r', encoding='utf-8') as f:
    new_data = json.load(f)

# Load existing data
with open('src/SoPorHoje.App/Resources/Raw/online_meetings.json', 'r', encoding='utf-8') as f:
    existing = json.load(f)

# Normalize day names
day_map = {
    'domingo': 'Domingo',
    'segunda': 'Segunda-feira', 'segunda-feira': 'Segunda-feira',
    'terça': 'Terça-feira', 'terça-feira': 'Terça-feira', 'terca': 'Terça-feira', 'terca-feira': 'Terça-feira',
    'quarta': 'Quarta-feira', 'quarta-feira': 'Quarta-feira',
    'quinta': 'Quinta-feira', 'quinta-feira': 'Quinta-feira',
    'sexta': 'Sexta-feira', 'sexta-feira': 'Sexta-feira',
    'sábado': 'Sábado', 'sabado': 'Sábado',
}

def normalize_day(d):
    return day_map.get(d.strip().lower(), d.strip())

# Normalize existing days
for r in existing['reunioes']:
    r['dia_semana'] = normalize_day(r.get('dia_semana', ''))

# Build index of existing: (group_name_lower, day, start_time) -> exists
existing_keys = set()
for r in existing.get('reunioes', []):
    day = r['dia_semana']
    for s in r.get('sessoes', []):
        key = (r['nome_grupo'].strip().lower(), day, s['horario_inicio'])
        existing_keys.add(key)

print(f"Existing: {len(existing.get('reunioes', []))} groups, {len(existing_keys)} sessions")

# Process new data - deduplicate ONLY by (group, day, start_time), NOT by URL
added = 0
updated = 0
skipped = 0

for nr in new_data.get('reunioes_permanentes', []):
    grupo = nr.get('grupo', '').strip()
    dia = normalize_day(nr.get('dia_semana', ''))
    h_inicio = nr.get('horario_inicio', '').strip()
    h_fim = nr.get('horario_fim', '').strip()
    url = nr.get('link_entrada', '').strip()
    plataforma = nr.get('plataforma', 'Zoom').strip()
    status = nr.get('status_sala', '').strip()
    tipo_reuniao = nr.get('tipo_reuniao', '').strip()

    if not grupo or not h_inicio or not url:
        skipped += 1
        continue

    # Normalize sala status
    if 'ABERTA' in status.upper():
        tipo_sessao = 'Aberta'
    elif 'FECHADA' in status.upper():
        tipo_sessao = 'Fechada'
    else:
        tipo_sessao = ''

    key = (grupo.lower(), dia, h_inicio)

    if key in existing_keys:
        skipped += 1
        continue

    # Find existing group for same name + day
    found = False
    for r in existing['reunioes']:
        if r['nome_grupo'].strip().lower() == grupo.lower() and r['dia_semana'] == dia:
            r['sessoes'].append({
                'horario_inicio': h_inicio,
                'horario_fim': h_fim,
                'tipo': tipo_sessao,
                'aplicativo': plataforma,
                'url': url
            })
            found = True
            updated += 1
            break

    if not found:
        existing['reunioes'].append({
            'nome_grupo': grupo,
            'dia_semana': dia,
            'localizacao': '',
            'observacoes': tipo_reuniao if tipo_reuniao else None,
            'sessoes': [{
                'horario_inicio': h_inicio,
                'horario_fim': h_fim,
                'tipo': tipo_sessao,
                'aplicativo': plataforma,
                'url': url
            }]
        })
        added += 1

    existing_keys.add(key)

# Sort groups by day order then name
day_order = {'Domingo': 0, 'Segunda-feira': 1, 'Terça-feira': 2, 'Quarta-feira': 3, 'Quinta-feira': 4, 'Sexta-feira': 5, 'Sábado': 6}
existing['reunioes'].sort(key=lambda r: (day_order.get(r['dia_semana'], 99), r['nome_grupo']))

# Count totals
total_groups = len(existing['reunioes'])
total_sessions = sum(len(r['sessoes']) for r in existing['reunioes'])

day_counts = {}
for r in existing['reunioes']:
    d = r['dia_semana']
    day_counts[d] = day_counts.get(d, 0) + len(r['sessoes'])

print(f"\nMerge results:")
print(f"  Added: {added} new groups")
print(f"  Updated: {updated} existing groups (new sessions)")
print(f"  Skipped: {skipped} (duplicates or invalid)")
print(f"\nFinal totals:")
print(f"  Groups: {total_groups}")
print(f"  Sessions: {total_sessions}")
print(f"\nBy day:")
for d in ['Domingo','Segunda-feira','Terça-feira','Quarta-feira','Quinta-feira','Sexta-feira','Sábado']:
    print(f"  {d}: {day_counts.get(d, 0)}")

with open('src/SoPorHoje.App/Resources/Raw/online_meetings.json', 'w', encoding='utf-8') as f:
    json.dump(existing, f, ensure_ascii=False, indent=2)

print("\nSaved!")
