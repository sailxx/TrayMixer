// Тексты картинок README на всех языках. Порядок языков — как в переключателе README.
using System.Collections.Generic;

class Lang
{
    public string Code, Alt, Line1, Line2, Cta, FeaturesTitle, FeaturesAlt, NumbersTitle, NumbersAlt, SecurityTitle, SecurityAlt;
    public string[] Chips;
    public string[][] Features; // заголовок, описание (переносится автоматически)
    public string[][] Numbers;  // число, подпись
    public string[] Security;
}

static class Langs
{
    public static readonly List<Lang> All = new List<Lang>
    {
        new Lang
        {
            Code = "ru",
            Alt = "TrayMixer — громкость каждого устройства и приложения в одном окне Windows 11",
            Line1 = "Громкость каждого устройства и приложения", Line2 = "в одном окне в стиле Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 МБ памяти", "0% CPU", "Без установки", "Открытый код" },
            Cta = "Скачать для Windows 11",
            FeaturesTitle = "ВОЗМОЖНОСТИ", FeaturesAlt = "Возможности TrayMixer",
            Features = new[]
            {
                new[] { "Все устройства", "Наушники, колонки, монитор — у каждого свой ползунок. Клик по имени делает его основным." },
                new[] { "Каждое приложение", "Браузер, игра и Discord — отдельно, с их иконками. Несколько вкладок = одна строка." },
                new[] { "Родной Windows 11", "Mica или Acrylic, скруглённые углы, тёмная и светлая тема, ваш цвет акцента." },
                new[] { "Ничего лишнего", "Правый клик — скрыть строку. Вернуть можно в меню трея. Всё запоминается." },
                new[] { "Мгновенно и легко", "Слушает события Windows вместо опроса: 0% CPU в простое и около 2 МБ памяти." },
                new[] { "Безопасно", "Без интернета и прав админа. Открытый код, сборка на GitHub с подтверждением происхождения." },
            },
            NumbersTitle = "ЛЁГКИЙ, КАК СИСТЕМНЫЙ", NumbersAlt = "TrayMixer в цифрах",
            Numbers = new[] { new[] { "~2 МБ", "памяти в простое" }, new[] { "0%", "CPU без опроса" }, new[] { "165 КБ", "один exe-файл" }, new[] { "0", "зависимостей" } },
            SecurityTitle = "Безопасность", SecurityAlt = "Безопасность TrayMixer",
            Security = new[]
            {
                "Работает без прав администратора (манифест asInvoker)",
                "Нет сети: не отправляет и не скачивает ничего",
                "Системные DLL только из System32 — защита от подмены",
                "Релизы собирает GitHub Actions + SHA-256 и attestation",
                "Проверка кода CodeQL при каждом изменении",
                "Настройки — только в HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "en",
            Alt = "TrayMixer — volume for every device and app in one Windows 11 flyout",
            Line1 = "Volume for every device and every app", Line2 = "in one native Windows 11 flyout",
            Chips = new[] { "Mica · Acrylic", "~2 MB RAM", "0% CPU", "No install", "Open source" },
            Cta = "Download for Windows 11",
            FeaturesTitle = "FEATURES", FeaturesAlt = "TrayMixer features",
            Features = new[]
            {
                new[] { "Every device", "Headphones, speakers, monitor — each gets its own slider. Click a name to make it the default." },
                new[] { "Every app", "Browser, game and Discord — separately, with their icons. Many tabs = one row." },
                new[] { "Native Windows 11", "Mica or Acrylic, rounded corners, dark and light theme, your accent color." },
                new[] { "Nothing extra", "Right-click a row to hide it. Bring it back from the tray menu. Everything is remembered." },
                new[] { "Instant and light", "Listens to Windows events instead of polling: 0% CPU when idle and about 2 MB of RAM." },
                new[] { "Secure", "No internet, no admin rights. Open source, built on GitHub with provenance attestation." },
            },
            NumbersTitle = "AS LIGHT AS A SYSTEM APP", NumbersAlt = "TrayMixer by the numbers",
            Numbers = new[] { new[] { "~2 MB", "RAM when idle" }, new[] { "0%", "CPU, no polling" }, new[] { "165 KB", "a single exe" }, new[] { "0", "dependencies" } },
            SecurityTitle = "Security", SecurityAlt = "TrayMixer security",
            Security = new[]
            {
                "Runs without admin rights (asInvoker manifest)",
                "No network: never sends or downloads anything",
                "System DLLs from System32 only — no DLL hijacking",
                "Releases built by GitHub Actions + SHA-256 & attestation",
                "CodeQL code scanning on every change",
                "Settings live only in HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "es",
            Alt = "TrayMixer — el volumen de cada dispositivo y aplicación en una sola ventana de Windows 11",
            Line1 = "El volumen de cada dispositivo y aplicación", Line2 = "en una sola ventana al estilo de Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 MB de RAM", "0% CPU", "Sin instalar", "Código abierto" },
            Cta = "Descargar para Windows 11",
            FeaturesTitle = "FUNCIONES", FeaturesAlt = "Funciones de TrayMixer",
            Features = new[]
            {
                new[] { "Todos los dispositivos", "Auriculares, altavoces, monitor: cada uno con su control. Un clic en el nombre lo vuelve predeterminado." },
                new[] { "Cada aplicación", "Navegador, juego y Discord por separado, con sus iconos. Varias pestañas = una fila." },
                new[] { "Windows 11 nativo", "Mica o Acrylic, esquinas redondeadas, tema oscuro y claro, tu color de acento." },
                new[] { "Nada que sobre", "Clic derecho para ocultar una fila. Se recupera desde el menú de la bandeja. Todo se recuerda." },
                new[] { "Rápido y ligero", "Escucha los eventos de Windows en lugar de consultar: 0% de CPU en reposo y unos 2 MB de RAM." },
                new[] { "Seguro", "Sin internet ni permisos de administrador. Código abierto, compilado en GitHub con atestación de origen." },
            },
            NumbersTitle = "TAN LIGERO COMO EL SISTEMA", NumbersAlt = "TrayMixer en cifras",
            Numbers = new[] { new[] { "~2 MB", "de RAM en reposo" }, new[] { "0%", "CPU, sin sondeo" }, new[] { "165 KB", "un solo exe" }, new[] { "0", "dependencias" } },
            SecurityTitle = "Seguridad", SecurityAlt = "Seguridad de TrayMixer",
            Security = new[]
            {
                "Sin permisos de administrador (manifiesto asInvoker)",
                "Sin red: no envía ni descarga nada",
                "DLL del sistema solo desde System32, sin suplantación",
                "Versiones compiladas en GitHub Actions + SHA-256",
                "Análisis de código CodeQL en cada cambio",
                "Ajustes solo en HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "pt",
            Alt = "TrayMixer — o volume de cada dispositivo e aplicativo em uma janela do Windows 11",
            Line1 = "O volume de cada dispositivo e aplicativo", Line2 = "em uma única janela no estilo do Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 MB de RAM", "0% CPU", "Sem instalar", "Código aberto" },
            Cta = "Baixar para Windows 11",
            FeaturesTitle = "RECURSOS", FeaturesAlt = "Recursos do TrayMixer",
            Features = new[]
            {
                new[] { "Todos os dispositivos", "Fones, caixas de som, monitor — cada um com seu controle. Um clique no nome o torna o padrão." },
                new[] { "Cada aplicativo", "Navegador, jogo e Discord separados, com seus ícones. Várias abas = uma linha." },
                new[] { "Windows 11 nativo", "Mica ou Acrylic, cantos arredondados, tema escuro e claro, sua cor de destaque." },
                new[] { "Nada sobrando", "Clique direito para ocultar uma linha. Ela volta pelo menu da bandeja. Tudo fica salvo." },
                new[] { "Rápido e leve", "Ouve os eventos do Windows em vez de consultar: 0% de CPU em repouso e cerca de 2 MB de RAM." },
                new[] { "Seguro", "Sem internet nem direitos de administrador. Código aberto, compilado no GitHub com atestado de origem." },
            },
            NumbersTitle = "LEVE COMO UM APP DO SISTEMA", NumbersAlt = "TrayMixer em números",
            Numbers = new[] { new[] { "~2 MB", "de RAM em repouso" }, new[] { "0%", "CPU, sem consultas" }, new[] { "165 KB", "um único exe" }, new[] { "0", "dependências" } },
            SecurityTitle = "Segurança", SecurityAlt = "Segurança do TrayMixer",
            Security = new[]
            {
                "Sem direitos de administrador (manifesto asInvoker)",
                "Sem rede: não envia nem baixa nada",
                "DLLs do sistema só do System32, sem sequestro de DLL",
                "Versões compiladas no GitHub Actions + SHA-256",
                "Análise de código CodeQL a cada mudança",
                "Configurações só em HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "de",
            Alt = "TrayMixer — Lautstärke jedes Geräts und jeder App in einem Windows-11-Fenster",
            Line1 = "Lautstärke für jedes Gerät und jede App", Line2 = "in einem Fenster im Windows-11-Stil",
            Chips = new[] { "Mica · Acrylic", "~2 MB RAM", "0% CPU", "Ohne Installation", "Open Source" },
            Cta = "Für Windows 11 laden",
            FeaturesTitle = "FUNKTIONEN", FeaturesAlt = "Funktionen von TrayMixer",
            Features = new[]
            {
                new[] { "Alle Geräte", "Kopfhörer, Lautsprecher, Monitor — jedes mit eigenem Regler. Klick auf den Namen macht es zum Standard." },
                new[] { "Jede App", "Browser, Spiel und Discord getrennt, mit ihren Symbolen. Viele Tabs = eine Zeile." },
                new[] { "Echtes Windows 11", "Mica oder Acrylic, abgerundete Ecken, helles und dunkles Design, deine Akzentfarbe." },
                new[] { "Nichts Überflüssiges", "Rechtsklick blendet eine Zeile aus. Zurückholen im Tray-Menü. Alles wird gespeichert." },
                new[] { "Schnell und leicht", "Hört auf Windows-Ereignisse statt abzufragen: 0% CPU im Leerlauf und etwa 2 MB RAM." },
                new[] { "Sicher", "Kein Internet, keine Adminrechte. Open Source, auf GitHub gebaut mit Herkunftsnachweis." },
            },
            NumbersTitle = "SO LEICHT WIE EINE SYSTEM-APP", NumbersAlt = "TrayMixer in Zahlen",
            Numbers = new[] { new[] { "~2 MB", "RAM im Leerlauf" }, new[] { "0%", "CPU, kein Polling" }, new[] { "165 KB", "eine einzige exe" }, new[] { "0", "Abhängigkeiten" } },
            SecurityTitle = "Sicherheit", SecurityAlt = "Sicherheit von TrayMixer",
            Security = new[]
            {
                "Läuft ohne Adminrechte (Manifest asInvoker)",
                "Kein Netzwerk: sendet und lädt nichts",
                "System-DLLs nur aus System32 — kein DLL-Hijacking",
                "Releases von GitHub Actions + SHA-256 & Attestation",
                "CodeQL-Codeprüfung bei jeder Änderung",
                "Einstellungen nur in HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "fr",
            Alt = "TrayMixer — le volume de chaque appareil et application dans une fenêtre Windows 11",
            Line1 = "Le volume de chaque appareil et application", Line2 = "dans une seule fenêtre au style Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 Mo de RAM", "0 % CPU", "Sans installation", "Open source" },
            Cta = "Télécharger pour Windows 11",
            FeaturesTitle = "FONCTIONNALITÉS", FeaturesAlt = "Fonctionnalités de TrayMixer",
            Features = new[]
            {
                new[] { "Tous les appareils", "Casque, enceintes, écran : chacun son curseur. Un clic sur le nom en fait l’appareil par défaut." },
                new[] { "Chaque application", "Navigateur, jeu et Discord séparément, avec leurs icônes. Plusieurs onglets = une ligne." },
                new[] { "Windows 11 natif", "Mica ou Acrylic, coins arrondis, thème sombre et clair, votre couleur d’accent." },
                new[] { "Rien de superflu", "Clic droit pour masquer une ligne. Elle revient via le menu de la barre. Tout est mémorisé." },
                new[] { "Instantané et léger", "Écoute les événements de Windows au lieu d’interroger : 0 % CPU au repos et environ 2 Mo de RAM." },
                new[] { "Sécurisé", "Sans internet ni droits admin. Open source, compilé sur GitHub avec attestation de provenance." },
            },
            NumbersTitle = "AUSSI LÉGER QU’UNE APPLI SYSTÈME", NumbersAlt = "TrayMixer en chiffres",
            Numbers = new[] { new[] { "~2 Mo", "de RAM au repos" }, new[] { "0 %", "CPU, sans sondage" }, new[] { "165 Ko", "un seul exe" }, new[] { "0", "dépendance" } },
            SecurityTitle = "Sécurité", SecurityAlt = "Sécurité de TrayMixer",
            Security = new[]
            {
                "Sans droits administrateur (manifeste asInvoker)",
                "Aucun réseau : n’envoie et ne télécharge rien",
                "DLL système uniquement depuis System32",
                "Versions compilées par GitHub Actions + SHA-256",
                "Analyse du code CodeQL à chaque modification",
                "Réglages uniquement dans HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "it",
            Alt = "TrayMixer — il volume di ogni dispositivo e app in una finestra di Windows 11",
            Line1 = "Il volume di ogni dispositivo e di ogni app", Line2 = "in un’unica finestra in stile Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 MB di RAM", "0% CPU", "Senza installare", "Open source" },
            Cta = "Scarica per Windows 11",
            FeaturesTitle = "FUNZIONI", FeaturesAlt = "Funzioni di TrayMixer",
            Features = new[]
            {
                new[] { "Tutti i dispositivi", "Cuffie, casse, monitor: ognuno ha il suo cursore. Un clic sul nome lo rende predefinito." },
                new[] { "Ogni app", "Browser, gioco e Discord separati, con le loro icone. Tante schede = una riga." },
                new[] { "Windows 11 nativo", "Mica o Acrylic, angoli arrotondati, tema scuro e chiaro, il tuo colore d’accento." },
                new[] { "Niente di superfluo", "Clic destro per nascondere una riga. Si ripristina dal menu della tray. Tutto viene ricordato." },
                new[] { "Istantaneo e leggero", "Ascolta gli eventi di Windows invece di interrogarlo: 0% di CPU a riposo e circa 2 MB di RAM." },
                new[] { "Sicuro", "Niente internet né diritti di amministratore. Open source, compilato su GitHub con attestazione." },
            },
            NumbersTitle = "LEGGERO COME UN’APP DI SISTEMA", NumbersAlt = "TrayMixer in numeri",
            Numbers = new[] { new[] { "~2 MB", "di RAM a riposo" }, new[] { "0%", "CPU, niente polling" }, new[] { "165 KB", "un solo exe" }, new[] { "0", "dipendenze" } },
            SecurityTitle = "Sicurezza", SecurityAlt = "Sicurezza di TrayMixer",
            Security = new[]
            {
                "Senza diritti di amministratore (manifest asInvoker)",
                "Niente rete: non invia e non scarica nulla",
                "DLL di sistema solo da System32, niente DLL hijacking",
                "Release compilate da GitHub Actions + SHA-256",
                "Analisi del codice CodeQL a ogni modifica",
                "Impostazioni solo in HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "tr",
            Alt = "TrayMixer — her cihazın ve uygulamanın sesi tek bir Windows 11 penceresinde",
            Line1 = "Her cihazın ve her uygulamanın sesi", Line2 = "Windows 11 tarzında tek bir pencerede",
            Chips = new[] { "Mica · Acrylic", "~2 MB RAM", "%0 CPU", "Kurulum yok", "Açık kaynak" },
            Cta = "Windows 11 için indir",
            FeaturesTitle = "ÖZELLİKLER", FeaturesAlt = "TrayMixer özellikleri",
            Features = new[]
            {
                new[] { "Tüm cihazlar", "Kulaklık, hoparlör, monitör — her birinin kendi kaydırıcısı var. Ada tıklayınca varsayılan olur." },
                new[] { "Her uygulama", "Tarayıcı, oyun ve Discord ayrı ayrı, kendi simgeleriyle. Birçok sekme = tek satır." },
                new[] { "Yerel Windows 11", "Mica veya Acrylic, yuvarlak köşeler, koyu ve açık tema, vurgu renginiz." },
                new[] { "Fazlası yok", "Sağ tıkla satırı gizle. Tepsi menüsünden geri getir. Her şey hatırlanır." },
                new[] { "Anında ve hafif", "Sorgulamak yerine Windows olaylarını dinler: boştayken %0 CPU ve yaklaşık 2 MB RAM." },
                new[] { "Güvenli", "İnternet ve yönetici hakkı yok. Açık kaynak, kaynağı doğrulanmış GitHub derlemesi." },
            },
            NumbersTitle = "SİSTEM UYGULAMASI KADAR HAFİF", NumbersAlt = "Rakamlarla TrayMixer",
            Numbers = new[] { new[] { "~2 MB", "boştayken RAM" }, new[] { "%0", "CPU, sorgulama yok" }, new[] { "165 KB", "tek bir exe" }, new[] { "0", "bağımlılık" } },
            SecurityTitle = "Güvenlik", SecurityAlt = "TrayMixer güvenliği",
            Security = new[]
            {
                "Yönetici hakkı olmadan çalışır (asInvoker manifesti)",
                "Ağ yok: hiçbir şey göndermez ve indirmez",
                "Sistem DLL'leri yalnızca System32'den alınır",
                "Sürümler GitHub Actions'ta derlenir + SHA-256",
                "Her değişiklikte CodeQL kod taraması",
                "Ayarlar yalnızca HKCU\\Software\\TrayMixer'da",
            },
        },
        new Lang
        {
            Code = "uk",
            Alt = "TrayMixer — гучність кожного пристрою й застосунку в одному вікні Windows 11",
            Line1 = "Гучність кожного пристрою і застосунку", Line2 = "в одному вікні в стилі Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 МБ пам’яті", "0% CPU", "Без встановлення", "Відкритий код" },
            Cta = "Завантажити для Windows 11",
            FeaturesTitle = "МОЖЛИВОСТІ", FeaturesAlt = "Можливості TrayMixer",
            Features = new[]
            {
                new[] { "Усі пристрої", "Навушники, колонки, монітор — у кожного свій повзунок. Клік по назві робить його основним." },
                new[] { "Кожен застосунок", "Браузер, гра й Discord — окремо, з їхніми іконками. Кілька вкладок = один рядок." },
                new[] { "Рідний Windows 11", "Mica або Acrylic, заокруглені кути, темна і світла тема, ваш колір акценту." },
                new[] { "Нічого зайвого", "Правий клік — сховати рядок. Повернути можна в меню трею. Усе запам’ятовується." },
                new[] { "Миттєво і легко", "Слухає події Windows замість опитування: 0% CPU у простої та близько 2 МБ пам’яті." },
                new[] { "Безпечно", "Без інтернету й прав адміна. Відкритий код, збірка на GitHub із підтвердженням походження." },
            },
            NumbersTitle = "ЛЕГКИЙ, ЯК СИСТЕМНИЙ", NumbersAlt = "TrayMixer у цифрах",
            Numbers = new[] { new[] { "~2 МБ", "пам’яті в простої" }, new[] { "0%", "CPU без опитування" }, new[] { "165 КБ", "один exe-файл" }, new[] { "0", "залежностей" } },
            SecurityTitle = "Безпека", SecurityAlt = "Безпека TrayMixer",
            Security = new[]
            {
                "Працює без прав адміністратора (маніфест asInvoker)",
                "Без мережі: нічого не надсилає й не завантажує",
                "Системні DLL лише з System32 — захист від підміни",
                "Релізи збирає GitHub Actions + SHA-256 та attestation",
                "Перевірка коду CodeQL під час кожної зміни",
                "Налаштування — лише в HKCU\\Software\\TrayMixer",
            },
        },
        new Lang
        {
            Code = "pl",
            Alt = "TrayMixer — głośność każdego urządzenia i aplikacji w jednym oknie Windows 11",
            Line1 = "Głośność każdego urządzenia i aplikacji", Line2 = "w jednym oknie w stylu Windows 11",
            Chips = new[] { "Mica · Acrylic", "~2 MB RAM", "0% CPU", "Bez instalacji", "Otwarty kod" },
            Cta = "Pobierz dla Windows 11",
            FeaturesTitle = "FUNKCJE", FeaturesAlt = "Funkcje TrayMixer",
            Features = new[]
            {
                new[] { "Wszystkie urządzenia", "Słuchawki, głośniki, monitor — każde ma własny suwak. Kliknięcie nazwy ustawia je jako domyślne." },
                new[] { "Każda aplikacja", "Przeglądarka, gra i Discord osobno, z ich ikonami. Wiele kart = jeden wiersz." },
                new[] { "Natywny Windows 11", "Mica lub Acrylic, zaokrąglone rogi, ciemny i jasny motyw, twój kolor akcentu." },
                new[] { "Nic zbędnego", "Prawy klik ukrywa wiersz. Przywrócisz go z menu w zasobniku. Wszystko jest zapamiętywane." },
                new[] { "Szybki i lekki", "Nasłuchuje zdarzeń Windows zamiast odpytywać: 0% CPU w spoczynku i około 2 MB RAM." },
                new[] { "Bezpieczny", "Bez internetu i praw administratora. Otwarty kod, budowany na GitHub z poświadczeniem pochodzenia." },
            },
            NumbersTitle = "LEKKI JAK APLIKACJA SYSTEMOWA", NumbersAlt = "TrayMixer w liczbach",
            Numbers = new[] { new[] { "~2 MB", "RAM w spoczynku" }, new[] { "0%", "CPU, bez odpytywania" }, new[] { "165 KB", "jeden plik exe" }, new[] { "0", "zależności" } },
            SecurityTitle = "Bezpieczeństwo", SecurityAlt = "Bezpieczeństwo TrayMixer",
            Security = new[]
            {
                "Działa bez praw administratora (manifest asInvoker)",
                "Bez sieci: niczego nie wysyła ani nie pobiera",
                "Systemowe DLL tylko z System32 — bez podmiany DLL",
                "Wydania budowane w GitHub Actions + SHA-256",
                "Skanowanie kodu CodeQL przy każdej zmianie",
                "Ustawienia tylko w HKCU\\Software\\TrayMixer",
            },
        },
    };
}
