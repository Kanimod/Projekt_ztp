using System;
using System.Formats.Asn1;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

public enum TypTrybu
{
    FISZKA,
    QUIZ,
    WPISYWANIE
}

public enum StatusPrzetwarzania
{
    POPRAWNA_ODP,
    NIEPOPRAWNA_ODP,
    WYJDZ,
    NICNIEROB,
    NIEZNANAKOMENDA
}

// poprosze zeby wszystkie tryby nauki implementowaly ten interfejs
// 1. tryb nauki najpierw wysyla string z info o tym jak dziala do ui, ui go wyswietla
// 2. ZwrocPytanie generuje i zwraca pytanie testowe/quizowe cokolwiek
// 3. ui wysyla odpowiedz uzytkownika do trybu nauki, PrzetworzOdpowiedz ją well... przetwarza, tzn.
//      decyduje czy odpowiedz jest poprawna oraz czy uzytkownik przypadkiem nie chce wyjsc z trybu
// 4. powtarzamy kroki 2. i 3. az do momentu wyjscia uzytkownika z trybu  

public interface ITrybNauki
{
    public string ZwrocInfo();
    public string ZwrocPytanie();
    public StatusPrzetwarzania PrzetworzOdpowiedz(string odpowiedz);
}

public class TrybFiszka : TrybNaukiBase
{
    private int nrFiszki = 0;
    private Fiszka obecnaFiszka;
    private bool czyWszystkieOdwrocone = false;
    private bool czyObecnaOdwrocona = false;

    public TrybFiszka(FiszkaZestaw zestaw) : base(zestaw)
    {
        obecnaFiszka = zestaw.fiszki[nrFiszki];
    }
    public override string ZwrocInfo()
    {
        return "0 - Wyjdz z trybu\n" +
               "1 - Poprzednia Fiszka\n" +
               "2 - Nastepna Fiszka\n" +
               "3 - Odwróć obecną fiszkę\n" +
               "4 - Odwróc wszystkie fiszki\n";
    }
    public override string ZwrocPytanie()
    {
        bool pokazSlowo;
        if (czyObecnaOdwrocona) { pokazSlowo = czyWszystkieOdwrocone; }
        else { pokazSlowo = !czyWszystkieOdwrocone; }

        string obecneSlowo = pokazSlowo ? obecnaFiszka.slowo : obecnaFiszka.tlumaczenie;
        return $"Słowo nr. {nrFiszki + 1}: {obecneSlowo}:";
    }

    public override StatusPrzetwarzania PrzetworzOdpowiedz(string odpowiedz)
    {
        switch (odpowiedz)
        {
            case "0":
                return StatusPrzetwarzania.WYJDZ;

            case "1":
                nrFiszki = int.Clamp(nrFiszki - 1, 0, zestaw.fiszki.Count() - 1);
                obecnaFiszka = zestaw.fiszki[nrFiszki];
                czyObecnaOdwrocona = false;
                return StatusPrzetwarzania.NICNIEROB;

            case "2":
                nrFiszki = int.Clamp(nrFiszki + 1, 0, zestaw.fiszki.Count() - 1);
                obecnaFiszka = zestaw.fiszki[nrFiszki];
                czyObecnaOdwrocona = false;
                return StatusPrzetwarzania.NICNIEROB;

            case "3":
                czyObecnaOdwrocona = !czyObecnaOdwrocona;
                return StatusPrzetwarzania.NICNIEROB;

            case "4":
                czyWszystkieOdwrocone = !czyWszystkieOdwrocone;
                return StatusPrzetwarzania.NICNIEROB;

            default:
                if (odpowiedz == "") { return StatusPrzetwarzania.NICNIEROB; }
                return StatusPrzetwarzania.NIEZNANAKOMENDA;
        }
    }
}

public class TrybQuiz : TrybNaukiBase
{
    private int nrFiszki = 0;
    private Fiszka obecnaFiszka;
    private List<Fiszka> odpowiedzi;
    private int poprawnaOdpowiedz;

    Random rng = new Random();

    public TrybQuiz(FiszkaZestaw zestaw) : base(zestaw)
    {
        obecnaFiszka = zestaw.fiszki[nrFiszki];
    }
    public override string ZwrocInfo()
    {
        if (zestaw.fiszki.Count < 4)
        {
            return "Zestaw musi zawierać co najmniej 4 fiszki, aby korzystać z trybu quizu.";
        }
        else
        {
            return "0 - Wyjdz z trybu\n" +
                   "1 - Poprzednie słowo\n" +
                   "2 - Nastepne słowo\n" +
                   "Wybierz numer poprawnej odpowiedzi (3-6)\n";
        }
    }


    public override string ZwrocPytanie()
    {
        odpowiedzi = new List<Fiszka>();
        odpowiedzi.Add(obecnaFiszka);

        while (odpowiedzi.Count < 4)
        {
            int idx = rng.Next(zestaw.fiszki.Count);
            Fiszka losowa = zestaw.fiszki[idx];

            if (!odpowiedzi.Contains(losowa))
                odpowiedzi.Add(losowa);
        }

        odpowiedzi = odpowiedzi.OrderBy(x => rng.Next()).ToList();

        poprawnaOdpowiedz = odpowiedzi.IndexOf(obecnaFiszka);

        return
            $"Słowo nr. {nrFiszki + 1}: {obecnaFiszka.slowo}\n" +
            "Wybierz poprawne tłumaczenie:\n" +
            $"3. {odpowiedzi[0].tlumaczenie}\n" +
            $"4. {odpowiedzi[1].tlumaczenie}\n" +
            $"5. {odpowiedzi[2].tlumaczenie}\n" +
            $"6. {odpowiedzi[3].tlumaczenie}\n";
    }
    public override StatusPrzetwarzania PrzetworzOdpowiedz(string odpowiedz)
    {
        if (odpowiedz == "0")
        {
            return StatusPrzetwarzania.WYJDZ;
        }
        else if (odpowiedz == "1")
        {
            nrFiszki = int.Clamp(nrFiszki - 1, 0, zestaw.fiszki.Count() - 1);
            obecnaFiszka = zestaw.fiszki[nrFiszki];
            return StatusPrzetwarzania.NICNIEROB;
        }
        else if (odpowiedz == "2")
        {
            nrFiszki = int.Clamp(nrFiszki + 1, 0, zestaw.fiszki.Count() - 1);
            obecnaFiszka = zestaw.fiszki[nrFiszki];
            return StatusPrzetwarzania.NICNIEROB;
        }
        else if (Int32.Parse(odpowiedz) - 3 == poprawnaOdpowiedz)
        {
            Powiadom(obecnaFiszka, true);
            nrFiszki = int.Clamp(nrFiszki + 1, 0, zestaw.fiszki.Count() - 1);
            obecnaFiszka = zestaw.fiszki[nrFiszki];
            return StatusPrzetwarzania.POPRAWNA_ODP;
        }
        else if (int.TryParse(odpowiedz, out int num) && num >= 3 && num <= 6)
        {
            Powiadom(obecnaFiszka, false);
            return StatusPrzetwarzania.NIEPOPRAWNA_ODP;
        }
        else
        {
            return StatusPrzetwarzania.NIEZNANAKOMENDA;
        }



    }
}

public class TrybWpisywanie : TrybNaukiBase
{
    private int nrFiszki = 0;
    private Fiszka obecnaFiszka;
    private bool czyOdwrocone = false;

    public TrybWpisywanie(FiszkaZestaw zestaw) : base(zestaw)
    {
        obecnaFiszka = zestaw.fiszki[nrFiszki];
    }

    public override string ZwrocInfo()
    {
        return "0 - Wyjdz z trybu\n" +
               "1 - Poprzednie słowo\n" +
               "2 - Nastepne słowo\n" +
               "3 - Zmień kierunek (słowo↔tłumaczenie)\n" +
               "Wpisz odpowiedź i zatwierdź Enter\n";
    }

    public override string ZwrocPytanie()
    {
        string pokaz = czyOdwrocone ? obecnaFiszka.tlumaczenie : obecnaFiszka.slowo;
        return $"Słowo nr. {nrFiszki + 1}: {pokaz}\n" +
               "Wpisz poprawną odpowiedź:";
    }

    public override StatusPrzetwarzania PrzetworzOdpowiedz(string odpowiedz)
    {
        if (odpowiedz == null) odpowiedz = "";
        odpowiedz = odpowiedz.Trim();

        switch (odpowiedz)
        {
            case "0":
                return StatusPrzetwarzania.WYJDZ;

            case "1":
                nrFiszki = int.Clamp(nrFiszki - 1, 0, zestaw.fiszki.Count() - 1);
                obecnaFiszka = zestaw.fiszki[nrFiszki];
                return StatusPrzetwarzania.NICNIEROB;

            case "2":
                nrFiszki = int.Clamp(nrFiszki + 1, 0, zestaw.fiszki.Count() - 1);
                obecnaFiszka = zestaw.fiszki[nrFiszki];
                return StatusPrzetwarzania.NICNIEROB;

            case "3":
                czyOdwrocone = !czyOdwrocone;
                return StatusPrzetwarzania.NICNIEROB;

            default:
                if (odpowiedz == "")
                    return StatusPrzetwarzania.NICNIEROB;

                // oczekiwana odpowiedź zależy od kierunku
                string oczekiwana = czyOdwrocone ? obecnaFiszka.slowo : obecnaFiszka.tlumaczenie;

                bool ok = string.Equals(odpowiedz, oczekiwana, StringComparison.OrdinalIgnoreCase);

                // observer (statystyki)
                Powiadom(obecnaFiszka, ok);

                if (ok)
                {
                    nrFiszki = int.Clamp(nrFiszki + 1, 0, zestaw.fiszki.Count() - 1);
                    obecnaFiszka = zestaw.fiszki[nrFiszki];
                    return StatusPrzetwarzania.POPRAWNA_ODP;
                }

                return StatusPrzetwarzania.NIEPOPRAWNA_ODP;
        }
    }
}


public static class FabrykaTrybow
{
    public static ITrybNauki UtworzTryb(TypTrybu typ, FiszkaZestaw zestaw)
    {
        switch (typ)
        {
            case TypTrybu.FISZKA:
                return new TrybFiszka(zestaw);

            case TypTrybu.QUIZ:
                if (zestaw.fiszki.Count < 4)
                {
                    Console.WriteLine("Zestaw musi zawierać co najmniej 4 fiszki, aby korzystać z trybu quizu. Zwracam tryb fiszka.");
                    return new TrybFiszka(zestaw);
                }
                else
                {
                    return new TrybQuiz(zestaw);
                }

            case TypTrybu.WPISYWANIE:
                return new TrybWpisywanie(zestaw);

            default:
                Console.WriteLine("Niepoprawny typ trybu nauki podczas wyboru, zwracam trybfiszka");
                return new TrybFiszka(zestaw);
        }
    }
}

public interface IObserver
{
    void Aktualizuj(Fiszka fiszka, bool poprawnaOdpowiedz);
}

public interface ISubject
{
    void Subskrybuj(IObserver observer);
    void Odsubskrybuj(IObserver observer);
    void Powiadom(Fiszka fiszka, bool poprawnaOdpowiedz);
}

public abstract class TrybNaukiBase : ITrybNauki, ISubject
{
    protected FiszkaZestaw zestaw;
    private readonly List<IObserver> obserwatorzy = new();

    protected TrybNaukiBase(FiszkaZestaw zestaw)
    {
        this.zestaw = zestaw;
    }

    public void Subskrybuj(IObserver observer)
    {
        if (!obserwatorzy.Contains(observer))
            obserwatorzy.Add(observer);
    }

    public void Odsubskrybuj(IObserver observer)
    {
        obserwatorzy.Remove(observer);
    }

    public void Powiadom(Fiszka fiszka, bool poprawna)
    {
        foreach (var obs in obserwatorzy)
            obs.Aktualizuj(fiszka, poprawna);
    }

    public abstract string ZwrocInfo();
    public abstract string ZwrocPytanie();
    public abstract StatusPrzetwarzania PrzetworzOdpowiedz(string odpowiedz);
}

public class Statystyki : IObserver
{
    private readonly Dictionary<Fiszka, (int ok, int err)> dane = new();

    public void Aktualizuj(Fiszka fiszka, bool poprawna)
    {
        if (!dane.ContainsKey(fiszka))
            dane[fiszka] = (0, 0);

        var (ok, err) = dane[fiszka];
        if (poprawna) ok++; else err++;
        dane[fiszka] = (ok, err);
    }

    public string Raport()
    {
        if (dane.Count == 0) return "Brak statystyk.";

        var sb = new StringBuilder();
        sb.AppendLine("STATYSTYKI:");
        foreach (var d in dane)
            sb.AppendLine($"{d.Key.slowo} → Poprawne: {d.Value.ok}, Błędne: {d.Value.err}");
        return sb.ToString();
    }
}

public class PowtorkaBledow : IObserver
{
    private readonly List<Fiszka> bledneFiszki = new();

    public void Aktualizuj(Fiszka fiszka, bool poprawnaOdpowiedz)
    {
        if (!poprawnaOdpowiedz&&bledneFiszki.Contains(fiszka)==false)
            bledneFiszki.Add(fiszka);
    }

    public bool CzySaBledy()
        => bledneFiszki.Count > 0;

    public FiszkaZestaw UtworzZestawDoPowtorki()
    {
        return new FiszkaZestaw(bledneFiszki.ToList(),false)
        {
            nazwa = "Powtórka błędów",
            kategoria = "Automatyczna"
        };
    }
}



// Ta klasa NIE JEST napisana w dobrej praktyce programistycznej, ale zalezalo mi na czasie, a nie
// na wymyslaniu klas abstrakcyjnych zeby tylko durne ui moglo je implementowac
// z dobrych stron kod jest w miare przejrzysty i na pierwszy rzut oka widac w miare co sie dzieje
public class UI
{
    static void Main(string[] args)
    {
        MenuGlowne();
    }

    // Wyswietla cala poindeksowana liste i czeka na input (prosze pamietac ze w wyswietlanej liscie indeksy zaczynaja sie od 1)
    // Zwraca input uzytknowika (0 lub indeks wybranego elementu + 1)
    static int PrzejrzyjListeElementow<T>(List<T> lista, string opcjonalnyMessage = "")
    {
        string wybor = "x";
        Console.Clear();
        Console.WriteLine(opcjonalnyMessage + "Wszystkie Elementy: ");

        // Wypisuje elementy ze skladnia "1. Pierwszy element \n 2. Drugi element..."
        for (int i = 0; i < lista.Count(); i++)
        {
            Console.WriteLine($"{i + 1}. {lista[i]}");
        }

        // Prosi o input dopoki nie bedzie to liczba bedaca zerem lub jakims indeksem listy
        int parsedWybor;
        bool czyInputPoprawny;
        do
        {
            Console.Write("\nWybierz element (lub 0 aby wyjsc): ");
            Console.Write("> ");
            string input = Console.ReadLine();

            czyInputPoprawny = int.TryParse(input, out parsedWybor);

            if (!czyInputPoprawny || parsedWybor < 0 || parsedWybor > lista.Count)
            {
                Console.WriteLine($"Niepoprawny wybor, Podaj liczbe od 0 do {lista.Count}.");
                czyInputPoprawny = false;
            }

        } while (!czyInputPoprawny);

        return parsedWybor;
    }
    static void MenuGlowne()
    {
        bool menuWlaczone = true;
        string message =
            "0. Zamknij\n" +
            "1. Fiszki\n" +
            "2. Zestawy\n" +
            "3. Tryby Nauki\n" +
            "4. Statystyki\n" +
            "5. Test\n" +
            "6. Import/Eksport zestawow\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    Console.WriteLine("Zamykanie...");
                    menuWlaczone = false;
                    break;

                case "1":
                    MenuFiszki();
                    break;

                case "2":
                    MenuZestawy();
                    break;

                case "3":
                    MenuWyborTrybuNauki();
                    break;

                case "6":
                    MenuImportExport();
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }
    static void MenuFiszki()
    {
        bool menuWlaczone = true;
        string message =
            "0. Wyjdz\n" +
            "1. Przejrzyj wszystkie fiszki\n" +
            "2. Stworz nowa fiszke\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                    int wyborFiszki = PrzejrzyjListeElementow<Fiszka>(BazaFiszek.wszystkieFiszki);
                    if (wyborFiszki != 0)
                    {
                        Fiszka wybranaFiszka = BazaFiszek.wszystkieFiszki[wyborFiszki - 1];
                        MenuWyborFiszki(wybranaFiszka, BazaFiszek.wszystkieFiszki);
                    }
                    break;

                case "2":
                    string slowo, tlumaczenie;
                    Console.WriteLine("Podaj slowo:");
                    Console.Write("> ");
                    slowo = Console.ReadLine();
                    Console.WriteLine("Podaj tlumaczenie:");
                    Console.Write("> ");
                    tlumaczenie = Console.ReadLine();
                    BazaFiszek.wszystkieFiszki.Add(new Fiszka(slowo, tlumaczenie));
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }

    // calkiem generalne menu, uzywa go wybieranie fiszek z menu fiszek
    // oraz wybieranie fiszek z menu edycji zestawu
    static void MenuWyborFiszki(Fiszka wybranaFiszka, List<Fiszka> listaZawierajacaFiszke)
    {
        bool menuWlaczone = true;
        string message =
            $"Wybrana fiszka: {wybranaFiszka}\n" +
            "0. Wyjdz\n" +
            "1. Usun Fiszke\n" +
            "2. Edytuj Fiszke\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                    listaZawierajacaFiszke.Remove(wybranaFiszka);
                    menuWlaczone = false;
                    break;

                case "2":
                    string nowe_slowo, nowe_tlumaczenie;
                    Console.WriteLine($"Podaj slowo: (obecne: {wybranaFiszka.slowo})");
                    Console.Write("> ");
                    nowe_slowo = Console.ReadLine();

                    Console.WriteLine($"Podaj tlumaczenie: (obecne: {wybranaFiszka.tlumaczenie})");
                    Console.Write("> ");
                    nowe_tlumaczenie = Console.ReadLine();

                    wybranaFiszka.slowo = nowe_slowo;
                    wybranaFiszka.tlumaczenie = nowe_tlumaczenie;
                    menuWlaczone = false;
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }
    static void MenuZestawy()
    {
        bool menuWlaczone = true;
        string message =
            "0. Wyjdz\n" +
            "1. Przejrzyj wszystkie zestawy\n" +
            "2. Stworz nowy zestaw\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                    int wyborZestawu = PrzejrzyjListeElementow<FiszkaZestaw>(BazaZestawow.wszystkieZestawy);
                    if (wyborZestawu != 0)
                    {
                        FiszkaZestaw wybranyZestaw = BazaZestawow.wszystkieZestawy[wyborZestawu - 1];
                        MenuWyborZestawu(wybranyZestaw);
                    }
                    break;

                case "2":
                    int wyborFiszki = -1;
                    if (BazaFiszek.wszystkieFiszki.Count > 0)
                    {
                        FiszkaZestaw nowyZestaw = new FiszkaZestaw(new List<Fiszka>());
                        string nazwa, kategoria;
                        Console.WriteLine("Podaj nazwe nowego zestawu:");
                        Console.Write("> ");
                        nazwa = Console.ReadLine();

                        Console.WriteLine("Podaj kategorie:");
                        Console.Write("> ");
                        kategoria = Console.ReadLine();

                        nowyZestaw.nazwa = nazwa;
                        nowyZestaw.kategoria = kategoria;

                        while (wyborFiszki != 0)
                        {
                            wyborFiszki = PrzejrzyjListeElementow<Fiszka>(BazaFiszek.wszystkieFiszki, "Wybierz fiszki do: \n" + nowyZestaw + "\n");
                            if (wyborFiszki != 0)
                            {
                                Fiszka wybranaFiszka = BazaFiszek.wszystkieFiszki[wyborFiszki - 1];
                                nowyZestaw.DodajFiszke(wybranaFiszka);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("W bazie nie ma żadnych fiszek!");
                        Console.ReadLine();
                    }
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }
    static void MenuWyborZestawu(FiszkaZestaw wybranyZestaw)
    {
        bool menuWlaczone = true;
        string message =
            $"Wybrany zestaw: {wybranyZestaw}\n" +
            "0. Wyjdz\n" +
            "1. Usun Zestaw\n" +
            "2. Edytuj Zestaw\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                    BazaZestawow.UsunZestaw(wybranyZestaw);
                    menuWlaczone = false;
                    break;

                case "2":
                    MenuEdycjaZestawu(wybranyZestaw);
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }
    static void MenuEdycjaZestawu(FiszkaZestaw wybranyZestaw)
    {
        bool menuWlaczone = true;
        string message =
            $"Wybrany zestaw: {wybranyZestaw}\n" +
            "0. Wyjdz\n" +
            "1. Edytuj właściwości\n" +
            "2. Edytuj fiszki\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                    string nazwa, kategoria;
                    Console.WriteLine($"Podaj nowa nazwe zestawu: (obecna nazwa: {wybranyZestaw.nazwa})");
                    Console.Write("> ");
                    nazwa = Console.ReadLine();

                    Console.WriteLine($"Podaj nowa kategorie zestawu: (obecna kategoria: {wybranyZestaw.kategoria})");
                    Console.Write("> ");
                    kategoria = Console.ReadLine();

                    wybranyZestaw.nazwa = nazwa;
                    wybranyZestaw.kategoria = kategoria;
                    menuWlaczone = false;
                    break;

                case "2":
                    int wyborFiszki = -1;
                    while (wyborFiszki != 0)
                    {
                        wyborFiszki = PrzejrzyjListeElementow<Fiszka>(wybranyZestaw.fiszki, $"Wybierz fiszke z zestawu: {wybranyZestaw}" + "\n");
                        if (wyborFiszki != 0)
                        {
                            Fiszka wybranaFiszka = wybranyZestaw.fiszki[wyborFiszki - 1];
                            MenuWyborFiszki(wybranaFiszka, wybranyZestaw.fiszki);
                        }
                    }
                    menuWlaczone = false;
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }
    static void MenuWyborTrybuNauki()
    {
        bool menuWlaczone = true;
        string message =
            $"Wybierz tryb:\n" +
            "0. Wyjdz\n" +
            "1. Tryb Fiszek\n" +
            "2. Tryb Quizu\n" +
            "3. Tryb Wpisywania\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            int parsedWybor;
            bool czyWyborPoprawny = int.TryParse(wybor, out parsedWybor);

            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                case "2":
                case "3":
                    TypTrybu wybranyTyp = (TypTrybu)(int.Parse(wybor) - 1);
                    FiszkaZestaw wybranyZestaw;
                    int wyborZestawu = PrzejrzyjListeElementow<FiszkaZestaw>(BazaZestawow.wszystkieZestawy);
                    if (wyborZestawu != 0)
                    {
                        wybranyZestaw = BazaZestawow.wszystkieZestawy[wyborZestawu - 1];
                    }
                    else
                    {
                        break;
                    }

                    MenuTrybNauki(wybranyTyp, wybranyZestaw);
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.ReadLine();
                    break;
            }
        }
    }
    static void MenuTrybNauki(TypTrybu wybranyTyp, FiszkaZestaw wybranyZestaw)
    {
        ITrybNauki obecnyTryb = FabrykaTrybow.UtworzTryb(wybranyTyp, wybranyZestaw);
        bool trybWlaczony = true;
        var staty = new Statystyki();
        var powtorka = new PowtorkaBledow();

        if (obecnyTryb is TrybNaukiBase baza)
        {
            baza.Subskrybuj(staty);
            baza.Subskrybuj(powtorka);
        }


        while (trybWlaczony)
        {
            Console.Clear();
            Console.WriteLine(obecnyTryb.ZwrocInfo());
            Console.WriteLine(obecnyTryb.ZwrocPytanie());

            Console.Write("> ");
            string input = Console.ReadLine();
            StatusPrzetwarzania statusPrzetwarzania = obecnyTryb.PrzetworzOdpowiedz(input);

            switch (statusPrzetwarzania)
            {
                case StatusPrzetwarzania.POPRAWNA_ODP:
                    Console.WriteLine("Brawo! Poprawna odpowiedź!");
                    Console.ReadLine();
                    break;

                case StatusPrzetwarzania.NIEPOPRAWNA_ODP:
                    Console.WriteLine("Zła odpowiedź!");
                    Console.ReadLine();
                    break;

                case StatusPrzetwarzania.WYJDZ:
                    trybWlaczony = false;
                    break;

                case StatusPrzetwarzania.NICNIEROB:
                    break;

                case StatusPrzetwarzania.NIEZNANAKOMENDA:
                    Console.WriteLine("Niepoprawny input trybu nauki");
                    Console.ReadLine();
                    break;
            }

        }
        Console.WriteLine(staty.Raport());
        Console.ReadLine();

        if (powtorka.CzySaBledy())
        {
            Console.Clear();
            Console.WriteLine("Czy chcesz przejść do powtórki błędów?\n1. Tak\n2. Nie");
            Console.Write("> ");

            var decyzja = Console.ReadLine();

            if (decyzja == "1")
            {
                var zestawPowtorki = powtorka.UtworzZestawDoPowtorki();

                MenuTrybNauki(TypTrybu.WPISYWANIE, zestawPowtorki);
            }
        }
    }

    static void MenuImportExport()
    {
        string nazwaPliku;
        bool menuWlaczone = true;
        string message = "0. Wyjdz\n" +
                         "1. Importuj zestaw\n" +
                         "2. Eksportuj zestaw\n";

        while (menuWlaczone)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.Write("> ");
            string wybor = Console.ReadLine();
            switch (wybor)
            {
                case "0":
                    menuWlaczone = false;
                    break;

                case "1":
                    Console.WriteLine("Podaj nazwe pliku do importu:");
                    Console.Write("> ");
                    nazwaPliku = Console.ReadLine();

                    string path = Path.Combine(AppContext.BaseDirectory, nazwaPliku);

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("Nie mozna znalezc pliku");
                        Console.WriteLine("Nacisnij Enter, aby kontynuowac...");
                        Console.ReadLine();
                    }
                    else
                    {

                        ImportExport.Importuj(nazwaPliku);
                        Console.WriteLine("Import zakonczony pomyslnie");
                        Console.WriteLine("Nacisnij Enter, aby kontynuowac...");
                        Console.ReadLine();
                    }
                    break;

                case "2":
                    Console.WriteLine("Wybierz nazwe zestawu do eksportu:");
                    Console.Write("> ");
                    int i = 0;
                    while (i != BazaZestawow.wszystkieZestawy.Count)
                    {
                        Console.WriteLine($"{i + 1}. {BazaZestawow.wszystkieZestawy[i].nazwa}");
                        i++;
                    }
                    Console.Write("\n> ");
                    FiszkaZestaw nazwaZestawu = BazaZestawow.wszystkieZestawy[int.Parse(Console.ReadLine()) - 1];
                    nazwaPliku = nazwaZestawu.nazwa;
                    i = 0;
                    bool contains = false;
                    while (i != BazaZestawow.wszystkieZestawy.Count())
                    {
                        nazwaZestawu = BazaZestawow.wszystkieZestawy[i];
                        if (nazwaZestawu.nazwa == nazwaPliku)
                        {
                            Console.WriteLine("Pod jaka nazwe pliku eksport wykonac (bez rozszerzenia)?");
                            Console.Write("> ");
                            nazwaPliku = Console.ReadLine();
                            ImportExport.Eksportuj(nazwaZestawu, nazwaPliku);
                            Console.WriteLine("Eksport zakonczony pomyslnie");
                            Console.WriteLine("Nacisnij Enter, aby kontynuowac...");
                            Console.ReadLine();
                            contains = true;
                            break;
                        }
                        i++;
                    }
                    ;
                    if (!contains)
                    {
                        Console.WriteLine("Nie ma takiego zestawu");
                        Console.WriteLine("Nacisnij Enter, aby kontynuowac...");
                        Console.ReadLine();
                    }
                    break;

                default:
                    Console.WriteLine("Niepoprawna komenda");
                    Console.WriteLine("Nacisnij Enter, aby kontynuowac...");
                    Console.ReadLine();
                    break;
            }
        }
    }
}
static class BazaZestawow
{
    static public List<FiszkaZestaw> wszystkieZestawy = new List<FiszkaZestaw>();

    static public bool UsunZestaw(FiszkaZestaw zestawDoUsuniecia)
    {
        if (wszystkieZestawy.Contains(zestawDoUsuniecia))
        {
            wszystkieZestawy.Remove(zestawDoUsuniecia);
            return true;
        }
        else
        {
            Console.WriteLine("Nie ma takiego zestawu");
            return false;
        }
    }
}

static class BazaFiszek
{
    static public List<Fiszka> wszystkieFiszki = new List<Fiszka>();
    static public bool UsunFiszke(Fiszka fiszkaDoUsuniecia)
    {
        if (wszystkieFiszki.Contains(fiszkaDoUsuniecia))
        {
            wszystkieFiszki.Remove(fiszkaDoUsuniecia);
            return true;
        }
        else
        {
            Console.WriteLine("Nie ma takiej fiszki");
            return false;
        }
    }
}

public class Fiszka
{
    public string slowo;
    public string tlumaczenie;
    public bool czyZnana;
    public int poziomZnajomosci;

    public Fiszka(string slowo, string tlumaczenie)
    {
        this.slowo = slowo;
        this.tlumaczenie = tlumaczenie;
        //BazaFiszek.wszystkieFiszki.Add(this);  to powodowalo bledy bo odrazu te zestawy co importowalam uaaklualnialy baze fiszek
    }
    public override string ToString()
    {
        return $"{slowo} / {tlumaczenie}";
    }
}
public class FiszkaZestaw
{
    public List<Fiszka> fiszki;
    public string nazwa;
    public string kategoria;

    public FiszkaZestaw(List<Fiszka> fiszki, bool dodajDoBazy = true)
    {
        this.fiszki = fiszki;
        if(dodajDoBazy)
        BazaZestawow.wszystkieZestawy.Add(this);
    }

    public void DodajFiszke(Fiszka nowaFiszka)
    {
        fiszki.Add(nowaFiszka);
    }

    public bool UsunFiszke(Fiszka fiszkaDoUsuniecia)
    {
        if (fiszki.Contains(fiszkaDoUsuniecia))
        {
            fiszki.Remove(fiszkaDoUsuniecia);
            return true;
        }
        else
        {
            Console.WriteLine("nie ma takiej fiszki");
            return false;
        }
    }
    public override string ToString()
    {
        return $"Zestaw \"{nazwa}\" \n(kategoria {kategoria}, ilosc elementow: {fiszki.Count})";
    }
}

public class ImportExport
{

    static public void Importuj(string nazwaPliku)
    {
        List<Fiszka> fiszkiPomocnicze = new List<Fiszka>();
        FiszkaZestaw nowyZestaw = new FiszkaZestaw(fiszkiPomocnicze);
        Console.WriteLine("podaj nazwe zestawu:");
        Console.Write("> ");
        nowyZestaw.nazwa = Console.ReadLine();
        Console.WriteLine("podaj kategorie zestawu:");
        Console.Write("> ");
        nowyZestaw.kategoria = Console.ReadLine();
        string[] linie = File.ReadAllLines(nazwaPliku);
        foreach (string line in linie)
        {
            string[] czesci = line.Split(',');
            if (czesci.Length == 2)
            {
                string slowo = czesci[0].Trim();
                string tlumaczenie = czesci[1].Trim();
                Fiszka nowaFiszka = new Fiszka(slowo, tlumaczenie);
                fiszkiPomocnicze.Add(nowaFiszka);
            }
        }
    }

    static public void Eksportuj(FiszkaZestaw fiszkaZestawVar, string nazwaPliku)
    {
        using (StreamWriter writer = new StreamWriter(nazwaPliku + ".txt"))
        {
            foreach (Fiszka fiszka in fiszkaZestawVar.fiszki)
            {
                writer.WriteLine($"{fiszka.slowo}, {fiszka.tlumaczenie}");
            }
        }
    }
}
