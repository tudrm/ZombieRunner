# ZombieRunner — Main Menu

Data: 2026-11-02. Status: COMPLETE pentru implementarea vizuală și verificarea în Unity Editor.

## Canva și rezultatul vizual

Design separat, finalizat și salvat: [ZombieRunner — Main Menu](https://www.canva.com/design/DAHZtx05b50/fbo2POEL94nfbh7p0UISnA/edit), ID `DAHZtx05b50`. Tranzacția finală `4371264362098900528` a fost confirmată prin conector. Shop-ul original `DAHZsSxLLV0` nu a fost modificat.

Fundal: imaginea Canva `MAHZt3zcalM`, descărcată manual de utilizator în format JPG, 944×1680, fără text sau butoane. Fișierul-sursă din Downloads: `AaGZhES2bGVMAWTLjZm6hA-AaGZhES2icyhZvA5t5lziA.jpg`. Mockup-ul complet `ZombieRunner — Main Menu.png` rămâne doar referință, în `MainMenuCanvaReview/Canva-reference.png`.

În Unity, fundalul păstrează proporțiile și acoperă ecranul prin decupare centrală. Titlul și cele patru butoane sunt elemente uGUI/TextMeshPro independente. Stilul reutilizează fontul BarlowCondensed SDF, marginile geometrice și imaginile din Shop. Accentele lime și albul cald sunt completate de iluminarea amber a orașului. PLAY este acțiunea principală, cu Shop și Locker pe același rând și Settings dedesubt.

Comparația cu Canva a fost vizuală, nu pixel-perfect: fontul Unity este cel existent în Shop; înălțimea ecranului schimbă spațierea și decuparea. Fundalul original are rezoluția 944×1680 și este mărit pe ecranele cu mai mulți pixeli. Calitatea pe dispozitive fizice trebuie evaluată separat.

## Comportamentul butoanelor

| Buton | Legătură originală păstrată | Verificare |
|---|---|---|
| PLAY | `MainMenuManager.StartGame` → loading panel → `ScenaJoc` | PASS, tranziție reală în Play Mode |
| SHOP | `ShopManager.OpenShop` | PASS, deschidere și închidere prin X |
| LOCKER | `ShopManager.OpenShop` | PASS pentru comportamentul existent; inventarul separat nu există încă |
| SETTINGS | `SettingsMenu.Open` | PASS, deschidere și închidere prin butonul existent |

Butonul `ButonExit` din gameplay, conectat la `GameManager.IntoarceLaMeniu`, a readus MainMenu. Testele au invocat UnityEvents existente prin MCP. Centrul fiecărui buton a fost verificat și prin GraphicRaycaster. Nu s-a simulat o sesiune completă de joc; timpul a fost oprit imediat la încărcarea ScenaJoc, pentru a evita schimbarea progresului.

Stările Normal, Pressed, Selected și Disabled folosesc ColorTint, cu tranziție de 0,12 secunde. Configurația tuturor butoanelor a fost verificată; pe PLAY s-au verificat și culorile efectiv randate după pointer-down și dezactivare temporară. Interacțiunea normală a fost restaurată.

## Responsive și verificări

Game View: **1179×2556**, **1080×1920**, **720×1600**, **640×1536**, **1536×2048**. Toate au trecut verificările: fără suprapuneri, texte încadrate, butoane în safe area, dimensiuni logice de apăsare de minimum 60 unități și fundal nedistorsionat. Pentru fiecare rezoluție au fost simulate și margini de siguranță: 2,5% lateral, 4% jos, 6% sus. Acestea verifică geometria, nu înlocuiesc testarea unui notch fizic.

Rapoarte în `MainMenuCanvaReview`:

- `Navigation.txt`: navigare, UnityEvents, feedback, redeschiderea scenei și referințe.
- `Layouts.txt`: cele cinci rezoluții și safe area simulată.
- `Preservation.txt`: SHA256 identic pentru fișierele protejate; niciun obiect vechi eliminat și nicio modificare serializată în afara Main Menu.
- `PlayerPrefs-before.txt` / `PlayerPrefs-after.txt`: identice, inclusiv existența/absența cheilor. Coins = 100, Gems = 20, arma echipată = `pistol_B`; ownership, HighScore și volume audio păstrate.
- Capturi `1179x2556-MainMenu.png`, `1080x1920-MainMenu.png`, `720x1600-MainMenu.png`, `640x1536-MainMenu.png`, `1536x2048-MainMenu.png`.

Scena a fost salvată și redeschisă: referințele sunt intacte; pe controalele Main Menu nu există Missing Script. După QA, Play Mode a fost oprit, dimensiunile temporare Game View și `Application.runInBackground` au fost restaurate. Editorul a rămas în MainMenu, fără modificări nesalvate.

## Fișiere schimbate pentru această etapă

Modificate:

- `Assets/Scenes/MainMenu.unity`: numai aspectul Main Menu, titluri, etichete native, componenta de prezentare; restul panourilor păstrate.
- `Assets/MainMenuManager.cs`: referință opțională la prezentare și delegarea layout-ului; vechiul layout rămâne fallback. `StartGame` și încărcarea scenei sunt nemodificate.

Adăugate, împreună cu fișierele `.meta` corespunzătoare:

- `Assets/MainMenuPresentation.cs`: layout adaptiv și background cover, fără logică de gameplay/economie.
- `Assets/Editor/MainMenuCanvaAuthoring.cs`: instrumentul de aplicare folosit o singură dată; refuză reaplicarea dacă meniul este deja migrat. Nu trebuie rulat pentru testarea meniului.
- `Assets/UI/MainMenuCanva.meta` și `Assets/UI/MainMenuCanva/CityBackground.jpg` / `.meta`.

Documentație și dovezi: acest fișier, `MainMenuCanvaReview/` și `Backups/MainMenuCanva-2026-11-02/`.

Nu au fost create prefabs noi. Modificările Shop existente anterior în working tree nu fac parte din această etapă. ShopManager, WeaponPreview, ShopPresentation, ShopWeaponCard, SettingsMenu, ScenaJoc, WeaponCatalog și GemStore au rămas identice cu baseline-ul salvat înaintea Main Menu. Nu au fost schimbate Packages sau ProjectSettings; nu s-au făcut build, commit sau push.

## Backup și revenire

Backup: `D:\UnityProjects\ZombieRunner\Backups\MainMenuCanva-2026-11-02`.

Conține scena MainMenu de dinaintea acestei etape, MainMenuManager anterior, diferențele Git preexistente, hash-uri și legăturile UnityEvents. Scena din acest backup include Shop-ul deja îmbunătățit.

Pentru a reveni doar la meniul vizual anterior: oprește Play Mode, închide Unity și păstrează mai întâi o copie a scenei actuale. Copiază `Backups/MainMenuCanva-2026-11-02/MainMenu.unity` peste `Assets/Scenes/MainMenu.unity`, fără a înlocui fișierul `.meta`, apoi redeschide proiectul. MainMenuManager actual folosește automat vechiul layout când referința `presentation` lipsește; scripturile noi pot rămâne nefolosite. Nu restaura separat numai scriptul MainMenuManager vechi, deoarece instrumentul nou de authoring îi folosește câmpul `presentation`.

Nu folosi backup-ul Shop mai vechi pentru această revenire: ar putea elimina îmbunătățirile Shop deja existente.

## Console și limite

Nu au apărut erori noi de compilare sau de execuție ale Main Menu în verificări. Cele două erori existente ale serviciilor Unity AI (`SettingsResult`, `PointsBalanceResult`, `generators-beta.ai.unity.com`) rămân. La încărcarea ScenaJoc apar avertismente Missing Script, inclusiv pe obiecte Decal Projector; fișierul ScenaJoc este nemodificat. Nu s-au reparat componentele scenei de joc în această etapă.

NOT TESTED: telefon/tabletă fizică, build Android/iOS, performanță pe dispozitiv, landscape. Nu s-a adăugat un inventar separat și nu s-au schimbat alte meniuri sau mecanici. Canva oferă conceptul vizual static; feedback-ul interactiv este implementat în Unity.

## Cum verifici în Unity

1. În Project, deschide `Assets > Scenes > MainMenu` prin dublu-click. Nu porni scena Untitled.
2. Deschide Game View și alege o rezoluție portrait, de exemplu 1179×2556 sau 1080×1920.
3. Apasă Play în Editor. Verifică titlul, fundalul și feedback-ul la apăsare.
4. Apasă SHOP, apoi X pentru revenire. LOCKER trebuie să deschidă același Shop, conform funcției existente.
5. Apasă SETTINGS, apoi butonul de închidere. Sliderele existente sunt păstrate; modificarea lor manuală salvează valorile în mod normal.
6. Apasă PLAY din joc. Ecranul original de loading trebuie să încarce ScenaJoc. Pentru a încheia verificarea poți opri Play Mode din Editor. Revenirea din joc folosește butonul existent din panoul Game Over.
7. Oprește Play Mode înainte să editezi sau să salvezi scena. Nu rula din nou `Apply Canva Main Menu`.

Pentru modificări ulterioare: titlurile și butoanele sunt sub `Canvas/MenuSafeArea`, fundalul este `Canvas/MainMenu`, iar regulile responsive sunt în `MainMenuPresentation.cs`.
