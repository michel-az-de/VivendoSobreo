namespace SoPorHoje.App.Constants;

public static class ChipDefinitions
{
    public static readonly List<(int Days, string Name, string Label, string Color, string BgColor, string Emoji)> Chips = new()
    {
        (1,    "Amarela",         "Ingresso",  "#D4A017", "#2C2410", "\U0001f305"),
        (90,   "Azul",            "3 Meses",   "#2E86C1", "#0E1E2C", "\U0001f30a"),
        (180,  "Rosa",            "6 Meses",   "#C7588E", "#2A1420", "\U0001f338"),
        (270,  "Vermelha",        "9 Meses",   "#C0392B", "#2A1212", "\U0001f525"),
        (365,  "Verde",           "1 Ano",     "#27AE60", "#0E2A18", "\U0001f33f"),
        (730,  "Verde Gravata",   "2 Anos",    "#1E8449", "#0C2214", "\U0001f333"),
        (1825, "Branca Gravata",  "5 Anos",    "#D5D8DC", "#1E2024", "\u2b50"),
        (3650, "Amarela Gravata", "10 Anos",   "#D4A017", "#2C2410", "\U0001f3c6"),
        (5475, "Azul Gravata",    "15 Anos",   "#2E86C1", "#0E1E2C", "\U0001f48e"),
        (7300, "Rosa Gravata",    "20 Anos",   "#C7588E", "#2A1420", "\U0001f451"),
    };
}
