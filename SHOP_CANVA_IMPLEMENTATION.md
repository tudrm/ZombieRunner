# ZombieRunner — implementarea Shop Canva

## Stare: PARTIAL

Interfața funcțională este implementată în scena existentă `Assets/Scenes/MainMenu.unity`. Integrarea cu Shop-ul existent și verificările în Editor sunt încheiate. Fidelitatea grafică rămâne parțială: unele imagini Canva sunt thumbnails, iar fundalul și iluminarea armei nu reproduc integral ilustrația aprobată. Nu este declarată validare pentru publicare sau pe dispozitiv fizic.

Referință: [designul Canva aprobat](https://www.canva.com/design/DAHZsSxLLV0/1RBvtQRZ1R3Ay8ibJtjnGg/edit), ID `DAHZsSxLLV0`.

## Ce s-a implementat

- Header cu Coins și Gems alăturate, accesibile prin apăsare; ambele trimit la secțiunea Gems.
- Categorii Pistols, SMGs și Rifles, cu starea selectată lime și controale uGUI reale.
- Panou pentru arma selectată, modelul 3D existent rotativ, nume, tip, tier, Damage, RPM și DPS.
- Acțiuni BUY în Coins sau Gems, respectiv EQUIP/EQUIPPED, conectate la logica existentă.
- Carduri reutilizabile în două coloane, prețuri în ambele monede, contur de selecție, OWNED/EQUIPPED și contorul armelor deținute.
- Secțiunea Gems separată, cinci oferte; ultima ocupă toată lățimea. Cantitățile și prețurile existente sunt 200 / €2, 550 / €5, 1.200 / €10, 7.000 / €50, 16.000 / €100.
- Derulare verticală, layout adaptat lățimii și safe area, lățime maximă de lectură pe tabletă.

Paleta urmărește Canva: grafit, lime `#BEF24A`, amber pentru Coins, ruby pentru Gems și text alb-cenușiu. Titlurile folosesc Barlow Condensed Bold, cu licența OFL inclusă; textele secundare folosesc fontul TMP existent. Fontul nu este confirmat drept identic cu cel din Canva. Panourile folosesc sprites 9-slice, adică margini păstrate la redimensionare. Textul și butoanele rămân componente Unity, nu o captură statică.

Preview-ul prezintă modelele jocului, nu armele desenate în Canva. Materialele existente ale modelelor au un aspect diferit de ilustrația Canva. În asset-ul Hero, orașul și pistolul sunt o singură imagine; în Unity este folosit numai un decupaj lateral de fundal, pentru a evita afișarea unei arme statice în spatele modelului real.

## Funcționalitate și limite

Logica de cumpărare, ownership, echipare, prețurile, damage, fire rate și PlayerPrefs au fost păstrate. Nu s-au adăugat produse, reguli economice sau backend nou. Scena `ScenaJoc`, Settings, Locker și gameplay-ul nu au fost implementate sau modificate în această etapă. Fișierele relevante și ProjectSettings/Packages au fost comparate cu baseline și nu au diferențe de conținut.

Achizițiile cu euro nu sunt implementate în proiect. Mesajul `STORE UNAVAILABLE · Offers preview only` indică acest lucru; apăsarea ofertei afișează notificarea existentă și nu acordă Gems. Nu s-au simulat plăți.

## Fișiere și obiecte

Fișiere existente modificate:

| Fișier | Modificare |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Prezentarea Shop și referințele ei |
| `Assets/ShopManager.cs` | Legături cu stratul vizual nou; cumpărarea și persistența rămân existente |
| `Assets/WeaponPreview.cs` | Fundal transparent opțional și culoare configurabilă pentru lumina de contur |

Fișiere noi:

- `Assets/ShopPresentation.cs`: layout și stiluri pentru Shop.
- `Assets/ShopWeaponCard.cs`: aspectul selecției, ownership și prețurilor pe card.
- `Assets/Editor/CanvaShopAuthoring.cs`: instrumentul de migrare folosit; migrarea este deja aplicată și nu trebuie repetată.
- `Assets/Editor/CanvaShopChecks.cs`: verificări funcționale și de layout în Editor.
- `Assets/UI/ShopCanva/Prefabs/WeaponCard.prefab`: card reutilizabil.
- `Assets/UI/ShopCanva/Backup/OriginalShop.prefab`: copie vizuală a vechiului Shop.
- `Assets/UI/ShopCanva/Fonts/BarlowCondensed-Bold.ttf`, `BarlowCondensed SDF.asset`, `OFL.txt`.
- `Assets/UI/ShopCanva/ASSET_SOURCES.json`: proveniență și dimensiuni pentru imaginile Canva.
- `Assets/UI/ShopCanva/Art/`: `Coin.png`, `Gem.png`, `TabActive.png`, `Hero.png`, `BuyCoins.png`, `BuyGems.png`, `Card.png`, `Gems200.png`, `Gems550.png`, `Gems1200.png`, `Gems7000.png`, `Gems16000.png`; `Frame.png` și `ButtonNeutral.png` sunt elemente geometrice generate local pentru 9-slice.
- Fișierele `.meta` asociate și directoarelor noi, `Backups/ShopCanva-Baseline/`, `ShopCanvaReview/` și acest raport.

Obiectele afectate sunt sub `Canvas/Panel_Shop`: `ArmorySafeArea`, `Header` (Coins, Gems, ieșire), `ShopScroll/Viewport/Page` cu `Categories`, `SelectedWeapon`, `Weapons/Cards` și `GemStore/Offers`. Componenta `ShopManager` primește referința către `ShopPresentation`. Referințele și UnityEvents existente de deschidere, închidere, cumpărare și navigare au fost păstrate. Vechiul fundal al panoului este dezactivat.

## Rezultatele verificărilor

**PASS: 95 verificări funcționale; FAIL: 0 în raportul de regresie Shop.**

- Cele trei categorii și selecția tuturor celor 20 de arme; nume, Damage, RPM, RenderTexture și contur de selecție.
- Cumpărarea prin ambele butoane: debitarea monedei corecte, ownership și echipare; repetarea acțiunii nu taxează din nou.
- Fonduri insuficiente, EQUIP/EQUIPPED, închidere/redeschidere, notificările celor cinci oferte neconfigurate.
- UnityEvents și scurtăturile Coins/Gems.
- Restaurarea exactă a tuturor cheilor PlayerPrefs atinse de test, inclusiv cheile inițial absente. La final: 100 Coins, 50 Gems din valoarea implicită existentă (cheia Gems rămâne absentă), Outrider echipat.
- Reîncărcarea scenei păstrează starea; zero componente Shop lipsă și zero referințe ShopManager neatribuite, cu excepția provider-ului de plăți intenționat neconfigurat.
- Verificări de input în Editor: zonele Coins/Gems/X primesc raycast, drag-ul rotește preview-ul, drag-ul pe card ajunge la ScrollRect și deplasează conținutul.

| Rezoluție Game View | Rezultat |
|---|---|
| 1179 × 2556 | PASS |
| 1080 × 1920 | PASS |
| 720 × 1600 | PASS |
| 640 × 1536 | PASS — ecran îngust/înalt |
| 1536 × 2048 | PASS — tabletă |

Au fost verificate vizual capturile, overflow-ul TMP, limitele cardurilor și ofertelor, oferta pe toată lățimea și safe area cu insets simulate. Acestea sunt verificări în Editor, nu teste tactile pe telefon real.

**NOT TESTED:** touch pe dispozitiv fizic, notch real, performanță cu Profiler pe mobil, build, plăți reale/sandbox, regresie gameplay prin rularea `ScenaJoc`. Nu s-a construit sau publicat jocul.

Console: nu au apărut erori de compilare sau runtime Shop observate. Există două erori externe preexistente Unity AI, `SettingsResult` și `PointsBalanceResult`, către `generators-beta.ai.unity.com`. Nu au fost reparate în acest task. Avertismentul anterior despre Input Manager nu face parte din această implementare.

Rapoarte și capturi păstrate în `ShopCanvaReview/`:

- `Regression.txt`, `Layouts.txt`, `ReloadAndInput.txt`.
- `Shop-verified.png`: verificarea finală a butonului EQUIPPED și contorului OWNED.
- `1179x2556-Shop-final.png`, `1179x2556-Gems-final.png`, `1080x1920-SMG-final.png`, `720x1600-Rifles.png`, `640x1536-Narrow.png`, `1536x2048-Tablet.png`.

## Backup și recuperare

Baseline Git: `eba03dc` — `Backup inainte de modificarea shopului din Canva`. Nu s-a creat commit sau push automat.

`Backups/ShopCanva-Baseline/` conține copiile originale `MainMenu.unity`, `ShopManager.cs`, `WeaponPreview.cs` și `OriginalShop.zip`. Arhiva include și `Assets/Editor/ShopAuthoring.cs`. Pentru recuperare se compară și se restaurează numai fișierele Shop din baseline, cu Unity în afara Play Mode; nu este necesară resetarea întregului proiect.

`Assets/UI/ShopCanva/Backup/OriginalShop.prefab` păstrează vechea prezentare pentru comparație. Nu este o copie autonomă complet funcțională: avea și referințe către obiecte externe din scenă. Pentru rollback complet se folosește scena originală împreună cu scripturile din backup/baseline.

## Cum verifici în Unity

1. În fereastra Project deschide `Assets > Scenes` și dă dublu-click pe `MainMenu`. Nu folosi scena Untitled.
2. Deschide Game View și alege o rezoluție portrait, de exemplu 1179 × 2556 sau 1080 × 1920. Apasă Play, apoi SHOP din meniul jocului.
3. Apasă Pistols, SMGs și Rifles. Selectează carduri diferite: modelul, numele, statisticile și prețurile trebuie să se actualizeze.
4. Trage orizontal pe modelul armei pentru a-l roti. Derulează vertical pornind de pe carduri sau spațiul listei pentru a ajunge la restul armelor și oferte.
5. O armă deținută permite EQUIP; arma echipată arată EQUIPPED. Pentru una nedeținută sunt afișate BUY în Coins și Gems. Cumpărarea manuală, dacă ai suficiente fonduri, folosește soldul real salvat al jocului.
6. Apasă Coins sau Gems în header: lista trebuie să ajungă la oferte. Apasă o ofertă: trebuie să apară STORE UNAVAILABLE, fără schimbarea soldului.
7. Apasă X, redeschide SHOP și verifică arma echipată. Oprește Play, pornește din nou și verifică persistența.
8. Opțional, cu MainMenu în Play Mode, rulează `Tools > ZombieRunner > Check Canva Shop (Play Mode)`. Verificarea automată utilizează valori temporare și restaurează cheile PlayerPrefs în `finally`; rezultatele se scriu în `Library/CanvaShopValidation/Regression.txt`. Această comandă verifică funcționalitatea; matricea de rezoluții trebuie verificată separat în Game View.

Nu rula vechiul instrument de reconstruire Shop ca metodă de testare: poate regenera interfața veche. Nu este necesară repetarea migrării Canva.

## Asset-uri Canva: ce mai trebuie exportat

Conectorul oficial disponibil în această sesiune a oferit thumbnails, fără instrument de export PNG de producție. Fișierele au fost descărcate și importate efectiv; nu sunt declarate exporturi la rezoluție completă. Dimensiunile originale și cele returnate de conector sunt în `ASSET_SOURCES.json`. Hero, butoanele, tab-ul activ, cardul și unele pachete Gems au fost reduse. Contururile native nu înlocuiesc problema de rezoluție a ilustrațiilor.

Pași manuali pentru pregătirea exportului, fără a modifica designul aprobat:

1. Deschide designul Canva din linkul de la început și folosește `File > Make a copy` pentru o copie de export.
2. În copie, duplică pagina pentru fiecare asset. Folosește `Position > Layers` pentru a selecta elementele; păstrează pe fiecare pagină numai asset-ul de exportat. Pentru butoane/panouri păstrează doar fundalul, fără texte, prețuri sau iconițe suprapuse: acestea sunt componente Unity separate.
3. Pentru fiecare pagină folosește `Share > Download > PNG`, alege pagina respectivă și `Transparent background` dacă planul Canva permite. Descarcă la dimensiunea originală; nu micșora imaginea. Nu considera mărirea artificială o recuperare a detaliilor pierdute.
4. Decupează marginile transparente în editorul de imagini folosit pentru asset-uri și păstrează numele din manifest: `Coin`, `Gem`, `TabActive`, `BuyCoins`, `BuyGems`, `Card` și cele cinci imagini `Gems...`. Verifică exportul pe un fundal închis și unul deschis. Dacă planul nu oferă transparență, păstrează acest punct drept blocaj de export, fără fundal alb introdus în UI.
5. Hero necesită un fundal de oraș separat de pistol. Imaginea actuală le conține în același bitmap; ștergerea unui layer nu separă automat pistolul. Este necesar asset-ul original de fundal sau o versiune separată pregătită în copia Canva. Nu înlocui preview-ul 3D cu imaginea integrală a pistolului.
6. După verificarea exporturilor, înlocuiește fișierele PNG corespunzătoare din `Assets/UI/ShopCanva/Art/`, păstrând fișierele `.meta` și verificând decupajele și marginile 9-slice în Sprite Editor. Nu șterge asset-urile și nu le reimporta sub GUID-uri noi. Hero-ul curat necesită și ajustarea UV-ului de decupaj, deoarece implementarea actuală folosește numai marginea imaginii existente.

Canva documentează disponibilitatea exportului transparent în [Transparent background](https://www.canva.com/help/transparent-background/) și opțiunile formatelor în [Download file types](https://www.canva.com/help/download-file-types/). Fontul folosește [Barlow Condensed din Google Fonts](https://github.com/google/fonts/tree/main/ofl/barlowcondensed), cu licența copiată în proiect.

**Următorul pas necesar:** completarea exportului grafic la calitatea originală, în special fundalul Hero separat, apoi comparația vizuală finală cu Canva la rezoluția telefonului. Settings, Locker și plata reală nu fac parte din acest pas.
