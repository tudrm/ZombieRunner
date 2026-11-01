# Shop și Settings — ZombieRunner

## Verificare în Unity

1. În Project, deschide `Assets/Scenes/MainMenu.unity` prin dublu clic. Apasă Play în Editor, apoi SHOP.
2. Alege Pistols, SMGs sau Rifles. Selectează o armă și trage peste model pentru rotire. Pagina se derulează vertical; armele sunt urmate de ofertele Gems.
3. DAMAGE este damage-ul unui glonț, ROUNDS / MIN este limita de focuri pe minut, iar DAMAGE / SEC este damage-ul teoretic pe secundă. Diferențele verzi/roșii se raportează la arma echipată. Pistoalele necesită apăsări separate.
4. Pentru o armă blocată, apasă prețul în COINS sau GEMS. Se scade doar moneda aleasă și arma se echipează. Un preț este inactiv dacă soldul respectiv nu ajunge. O armă deținută are EQUIP; cea activă are EQUIPPED.
5. Apasă fie Coins, fie Gems în colțul de sus: pagina derulează până la ofertele Gems. Nu există pachete de Coins.
6. Pachetele afișate sunt 200 Gems / €2, 550 / €5, 1.200 / €10, 7.000 / €50 și 16.000 / €100. Bonusurile de 10%, 20%, 40%, 60% sunt raportate la rata de 100 Gems/euro a pachetului de €2.
7. Momentan, apăsarea unei oferte deschide mesajul STORE UNAVAILABLE. Nu se retrage bani și nu se acordă Gems: produsele nu sunt configurate într-un magazin real.
8. Închide Shop cu X. Apasă SETTINGS: SOUND EFFECTS controlează împușcăturile și efectele, iar MUSIC controlează separat viitoarea muzică. Încearcă 0%, apoi 50%, închide și redeschide meniul.
9. Din meniul principal apasă PLAY. `ScenaJoc` folosește arma echipată și volumele salvate. Pentru arme automate, ține apăsat butonul cu glonț. Pentru un test ușor al efectelor, setează SOUND EFFECTS la 0% înainte să intri în joc.
10. Oprește și repornește Play Mode pentru a verifica salvarea. În Game View încearcă iPhone SE Portrait, iPhone 15 și iPhone 11 Landscape. Nu este necesară reconstruirea UI.

## Date și extensii

- Modelele existente, cele 3 categorii, gameplay-ul și salvările au fost reutilizate.
- `Assets/Settings/WeaponCatalog.asset`: nume, modele, damage, roundsPerMinute, price (Coins), gemPrice (Gems). Prețul inițial în Gems este `ceil(price / 25)`. Câmpurile rămân editabile separat în Inspector.
- `Assets/Settings/GemStore.asset`: cele 5 oferte și ID-urile stabile ale produselor. Acestea sunt ID-uri propuse, nu produse deja înregistrate.
- `Assets/Settings/ShopHeading.asset`: font pentru titluri generat din `The Asesino.otf`, deja existent. Textul mic folosește Liberation Sans pentru lizibilitate.
- Portofelul păstrează cheile `MonedeSalvate` și `GemuriSalvate`; armele păstrează `ZR_Owned_<id>` și `ZR_EquippedWeapon`. Soldurile existente nu sunt resetate.
- `Assets/Resources/GameAudio.mixer` are grupurile Effects și Music, cu parametrii EffectsVolume și MusicVolume. Volumele originale ale armelor/surselor sunt păstrate; sliderul modifică volumul grupului.
- SFX existente din scenă și prefab-urile cu AudioSource sunt rutate în Effects. Pentru sunete noi: setează Output la Effects în AudioSource. Pentru muzică: alege Music. Alternativ adaugă `AudioChannel` pe prefab; bifează `music` numai pentru muzică. Componenta rutează sunetul la instanțiere, înainte de redare.
- Preferințele sunt `ZR_EffectsVolume` și `ZR_MusicVolume`; se aplică imediat și sunt salvate la închiderea Settings, pauză și ieșire. Muzica nu a fost adăugată în această etapă.

## Legarea viitorului magazin real

Este pregătit contractul `GemPurchaseProvider` și legătura lui cu UI, fără un SDK de plăți instalat sau tranzacții simulate. După alegerea platformei:

1. Creează în Google Play Console / App Store Connect produsele consumabile corespunzătoare ID-urilor din GemStore.
2. Integrează SDK-ul magazinului și implementează `GemPurchaseProvider` într-o componentă separată. Atribuie componenta în `ShopManager.purchaseProvider`.
3. `IsAvailable` devine true doar după conectare și încărcarea produselor. `LocalizedPrice` trebuie să returneze prețul real furnizat de magazin, cu moneda locală; UI îl folosește în locul valorii de prezentare în euro.
4. `Purchase` trebuie să trateze anularea, lipsa conexiunii și stările pending. Furnizorul verifică tranzacția/receipt-ul, acordă Gems, salvează și confirmă consumul. Păstrează un registru persistent de transaction IDs pentru a preveni acordarea repetată la retry/restart. Nu acorda Gems doar la apăsarea butonului sau pe baza unui boolean neverificat.
5. După finalizare, callback-ul actualizează portofelul și starea UI. La lansare trebuie procesate și tranzacțiile rămase pending. Pentru economia premium publicată, folosește validarea server-side și un portofel protejat; PlayerPrefs este salvarea locală actuală, nu un backend securizat.
6. Verifică tot fluxul cu conturi sandbox pe dispozitive înainte de lansare. Prețurile efective/taxele și disponibilitatea depind de configurarea magazinului.

## Testele proiectului

Din MainMenu în Play Mode, alege `Tools > ZombieRunner > Run Shop Integration Checks (Play Mode)`. Testul folosește armele, portofelele, butoanele, Mixer-ul și scenele reale, apoi restaurează cheile PlayerPrefs modificate, inclusiv preferințele audio. Nu interacționa cu jocul în timpul rulării. Rezultatul este în `Library/ShopIntegrationChecks.txt`.

`Tools > ZombieRunner > Validate Weapon Catalog` verifică datele catalogului. `Build Shop in Existing Scenes` reconstruiește și salvează UI în MainMenu și rutarea audio în ScenaJoc/prefab-uri; nu este necesar pentru a juca și poate suprascrie ajustările manuale de layout.

Verificarea este în Editor; nu reprezintă validarea pe dispozitive sau a plăților reale. Problemele vechi ale decorului/Decal Projector și serviciilor AI sunt separate de aceste schimbări.
