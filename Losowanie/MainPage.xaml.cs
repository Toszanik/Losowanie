


namespace Losowanie


{
    public partial class MainPage : ContentPage
    {
        private readonly Entry _imieEntry;
        private readonly Entry _klasaEntry;
        private readonly Button _dodajButton;
        private readonly ListView _listaKlasyListView;
        private readonly Button _losujButton;
        private readonly Button _edytujButton;
        private readonly Dictionary<string, List<string>> _listaUczniow = new();

        public MainPage()
        {
            _imieEntry = new Entry { Placeholder = "Imię" };
            _klasaEntry = new Entry { Placeholder = "Klasa" };
            _dodajButton = new Button { Text = "Dodaj" };
            _listaKlasyListView = new ListView();
            _losujButton = new Button { Text = "Losuj" };
            _edytujButton = new Button { Text = "Edytuj" };

            StackLayout stackLayout = new()
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = { _imieEntry, _klasaEntry, _dodajButton }
                    },
                    _listaKlasyListView,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = { _losujButton, _edytujButton }
                    }
                }
            };

            _dodajButton.Clicked += DodajUcznia;
            _losujButton.Clicked += LosujUcznia;
            _edytujButton.Clicked += EdytujListe;

            WczytajListeUczniow();

            Content = stackLayout;
        }

        private async void DodajUcznia(object sender, EventArgs e)
        {
            string imie = _imieEntry.Text;
            string klasa = _klasaEntry.Text;

            if (string.IsNullOrEmpty(imie) || string.IsNullOrEmpty(klasa))
            {
                await DisplayAlert("Błąd", "Wpisz imię i klasę", "OK");
                return;
            }

            if (!_listaUczniow.ContainsKey(klasa))
            {
                _listaUczniow.Add(klasa, new List<string>());
            }

            _listaUczniow[klasa].Add(imie);
            ZapiszListeUczniow();
            WczytajListeKlasy();

            _imieEntry.Text = "";
            _klasaEntry.Text = "";
        }
        private void ZapiszListeUczniow()
        {
            string plik = Path.Combine(FileSystem.AppDataDirectory, "listaUczniow.txt");

            using (StreamWriter writer = new StreamWriter(plik))
            {
                foreach (KeyValuePair<string, List<string>> kvp in _listaUczniow)
                {
                    foreach (string imie in kvp.Value)
                    {
                        writer.WriteLine($"{kvp.Key};{imie}");
                    }
                }
            }
        }

        private void WczytajListeKlasy()
        {
            _listaKlasyListView.ItemsSource = _listaUczniow.Keys.ToList();
        }
        private async void LosujUcznia(object sender, EventArgs e)
        {
            string wybranaKlasa = _listaKlasyListView.SelectedItem as string;

            if (string.IsNullOrEmpty(wybranaKlasa))
            {
                await DisplayAlert("Błąd", "Wybierz klasę", "OK");
                return;
            }

            if (_listaUczniow[wybranaKlasa].Count == 0)
            {
                await DisplayAlert("Błąd", "Brak uczniów w tej klasie", "OK");
                return;
            }

            Random random = new();
            int index = random.Next(_listaUczniow[wybranaKlasa].Count);
            string wybranyUczeń = _listaUczniow[wybranaKlasa][index];

            await DisplayAlert("Wylosowano", wybranyUczeń, "OK");
        }

        private async void EdytujListe(object sender, EventArgs e)
        {
            string wybranaKlasa = _listaKlasyListView.SelectedItem as string;

            if (string.IsNullOrEmpty(wybranaKlasa))
            {
                await DisplayAlert("Błąd", "Wybierz klasę", "OK");
                return;
            }

            List<string> uczniowieWKlasie = _listaUczniow[wybranaKlasa];

            var modalPage = new ContentPage();
            var listView = new ListView { ItemsSource = uczniowieWKlasie };
            var addButton = new Button { Text = "Dodaj ucznia" };
            var editButton = new Button { Text = "Edytuj ucznia" };

            addButton.Clicked += async (addSender, addArgs) =>
            {
                string noweImie = await DisplayPromptAsync("Dodaj ucznia", "Wpisz imię ucznia:");

                if (!string.IsNullOrWhiteSpace(noweImie))
                {
                    uczniowieWKlasie.Add(noweImie);
                    ZapiszListeUczniow();
                    listView.ItemsSource = null;
                    listView.ItemsSource = uczniowieWKlasie;
                }
            };

            editButton.Clicked += async (editSender, editArgs) =>
            {
                if (listView.SelectedItem == null)
                {
                    await DisplayAlert("Błąd", "Wybierz ucznia do edycji", "OK");
                    return;
                }

                string staryUczen = listView.SelectedItem as string;
                string noweImie = await DisplayPromptAsync("Edytuj ucznia", "Wpisz nowe imię ucznia:", initialValue: staryUczen);

                if (!string.IsNullOrWhiteSpace(noweImie))
                {
                    int index = uczniowieWKlasie.IndexOf(staryUczen);
                    uczniowieWKlasie[index] = noweImie;
                    ZapiszListeUczniow();
                    listView.ItemsSource = null;
                    listView.ItemsSource = uczniowieWKlasie;
                }
            };

            var stackLayout = new StackLayout
            {
                Children = { listView, addButton, editButton }
            };

            modalPage.Content = stackLayout;

            await Navigation.PushModalAsync(modalPage);
        }


        private void WczytajListeUczniow()
        {
            string plik = Path.Combine(FileSystem.AppDataDirectory, "listaUczniow.txt");

            if (!File.Exists(plik))
            {
                return;
            }

            string[] lines = File.ReadAllLines(plik);

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                string klasa = parts[0];
                string imie = parts[1];

                if (!_listaUczniow.ContainsKey(klasa))
                {
                    _listaUczniow.Add(klasa, new List<string>());
                }

                _listaUczniow[klasa].Add(imie);
            }

            WczytajListeKlasy();
        }
    }
}