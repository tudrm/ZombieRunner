# Shop — ZombieRunner

## Cum verifici

1. În fereastra Project, deschide `Assets/Scenes/MainMenu.unity` prin dublu clic.
2. Apasă Play în Unity, apoi SHOP. LOCKER deschide același arsenal.
3. Alege Pistols, SMGs sau Rifles. Selectează o armă și trage peste model pentru rotire. Derulează lista pentru toate armele.
4. Compară DAMAGE / HIT, FIRE RATE / RPM și DAMAGE / SEC cu arma echipată. RPM reprezintă numărul maxim de focuri pe minut; DPS este damage-ul teoretic pe secundă. La pistoale, fiecare foc necesită o apăsare.
5. UNLOCK & EQUIP cumpără și echipează arma. EQUIP WEAPON echipează gratuit o armă deja cumpărată. EQUIPPED și NOT ENOUGH COINS sunt stări inactive.
6. Închide Shop cu X, apoi apasă PLAY în meniul jocului. Se încarcă `ScenaJoc` cu arma aleasă. Folosește butonul cu glonț pentru a trage; pentru SMG/Rifle, ține-l apăsat.
7. Oprește și repornește Play Mode pentru a verifica salvarea. Poți deschide direct `Assets/Scenes/ScenaJoc.unity`; arma echipată este citită din aceeași salvare.
8. În Game View, încearcă rezoluții portrait și landscape. Interfața se adaptează și ține cont de `Screen.safeArea`.

## Conținut și economie

- 20 de modele existente: 10 Pistols, 5 SMGs, 5 Rifles.
- În fiecare categorie, tier-urile mai mari cresc damage-ul, RPM și prețul.
- SIDEARM este gratuit; o salvare nouă începe cu 1.500 Coins și 50 Gems. Soldurile deja salvate sunt păstrate.
- Fiecare zombie eliminat acordă 25 Coins, o singură dată.
- Armele se cumpără cu Coins. Gems sunt afișate din sistemul existent; nu există achiziții cu bani reali.
- Catalogul comun este `Assets/Settings/WeaponCatalog.asset`. Modifică în Inspector numele, prețul, damage-ul și roundsPerMinute. Păstrează ID-urile stabile pentru a nu pierde legătura cu armele cumpărate.
- Cheile existente `MonedeSalvate` și `GemuriSalvate` sunt reutilizate. Proprietatea folosește `ZR_Owned_<id>`, iar selecția `ZR_EquippedWeapon`.
- Datele Shop ajung la `WeaponManager`, apoi la `PlayerShoot` și `BulletMove`; `ZombieHealth` acordă recompensa.

## Verificări tehnice

În MainMenu, pornește Play Mode și alege `Tools > ZombieRunner > Run Shop Integration Checks (Play Mode)`. Lasă verificarea să termine înainte să oprești Play Mode. Testele cumpără temporar arme, schimbă scenele și trag proiectile reale; apoi restaurează cheile PlayerPrefs atinse. Nu interacționa cu jocul în timpul testelor. Rezultatul este în `Library/ShopIntegrationChecks.txt`.

Au trecut 135 de verificări în Unity 6000.3.6f1: progresia catalogului, deblocare, fonduri insuficiente, prevenirea plății duble, salvare între scene, toate modelele și statisticile, echiparea tuturor armelor, damage-ul proiectilelor, spawnerul existent, recompensa și blocarea tragerii în pauză.

Validarea catalogului este disponibilă și separat în `Tools > ZombieRunner > Validate Weapon Catalog`.

Nu este necesară reconstruirea interfeței: scenele sunt deja salvate. `Build Shop in Existing Scenes` este un instrument de authoring care reconstruiește interfața și salvează ambele scene; păstrează catalogul existent, dar suprascrie ajustările manuale ale layout-ului.

Validarea a fost făcută în Editor, nu printr-un build Android/iOS sau pe telefon. Avertismentele existente pentru componente lipsă din decor/Decal Projector și erorile serviciilor AI nu au fost reparate în această modificare. Butonul SETTINGS nu face parte din implementarea Shop.
