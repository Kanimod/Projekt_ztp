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
    NIEZNANAKOMENDA
}

// poprosze zeby wszystkie tryby nauki implementowaly ten interfejs
// 1. tryb nauki najpierw wysyla string z info o tym jak dziala do ui, ui go wyswietla
// 2. ZwrocTekst generuje i zwraca pytanie testowe/quizowe cokolwiek
// 3. ui wysyla odpowiedz uzytkownika do trybu nauki, PrzetworzOdpowiedz ją well... przetwarza, tzn.
//      decyduje czy odpowiedz jest poprawna oraz czy uzytkownik przypadkiem nie chce wyjsc z trybu
// 4. powtarzamy kroki 2. i 3. az do momentu wyjscia uzytkownika z trybu  

// najlepiej by bylo zrobic klase abstrakcyjna po ktora implementuje ten interfejs i po ktorej dziedzicza wszystkie tryby nauki,
// to wtedy troche mniej bedzie trzeba pisac tego samego kodu w kazdej z nich :)
public interface ITrybNauki
{
    public string ZwrocInfo();
    public string ZwrocTekst();
    public StatusPrzetwarzania PrzetworzOdpowiedz(string odpowiedz);
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
            Console.WriteLine($"{i+1}. {lista[i]}");
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
            "5. Test\n";

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
                    new Fiszka(slowo, tlumaczenie);
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
        BazaFiszek.wszystkieFiszki.Add(this);
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

    public FiszkaZestaw(List<Fiszka> fiszki)
    {
       this.fiszki = fiszki;
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