# Specifikation: SoMe Billede

## Formål

Appen lader brugeren vælge et billede fra sin computer, tilpasse det til faste billedformater for Facebook, Instagram og LinkedIn samt gemme de færdige billeder lokalt.

## Platform og teknologi

- Programmeringssprog: C#.
- Foreløbig platformantagelse: Windows-desktopapp.
- Appen skal bygges på en aktuel, understøttet version af .NET.
- Billedbehandling skal ske lokalt på brugerens computer. Billeder må ikke uploades til en ekstern tjeneste.
- Valg af UI-teknologi (for eksempel WPF, WinUI eller .NET MAUI) fastlægges under projektets tekniske design.

## Målgruppe

Brugere, der vil genbruge et eksisterende billede på sociale medier uden selv at skulle kende eller indtaste billeddimensioner.

## Brugerflow

1. Brugeren åbner appen og vælger et billede fra computeren.
2. Appen viser billedet og dets oprindelige dimensioner.
3. Brugeren vælger et eller flere outputformater via platform og indholdstype.
4. Appen viser en forhåndsvisning af hvert format.
5. Brugeren vælger, hvordan billedet skal tilpasses, og justerer beskæringen om nødvendigt.
6. Brugeren vælger filformat og destination.
7. Appen opretter en separat billedfil for hvert valgt format og viser, hvor filerne blev gemt.

## Understøttede billedstørrelser

Alle mål er bredde × højde i pixels.

| Platform | Indholdstype | Bredde | Højde |
|---|---|---:|---:|
| Facebook | Opslag | 1200 | 1200 |
| Facebook | Historier | 1080 | 1920 |
| Facebook | Coverbillede | 851 | 315 |
| Facebook | Profilfoto | 170 | 170 |
| Instagram | Opslag | 1080 | 1080 |
| Instagram | Historier | 1080 | 1920 |
| Instagram | Profilfoto | 320 | 320 |
| LinkedIn | Profilfoto | 400 | 400 |
| LinkedIn | Coverbillede | 1128 | 191 |
| LinkedIn | Opslag | 1080 | 1080 |

## Funktionelle krav

### Billedvalg og validering

- Brugeren kan vælge en lokal billedfil via en almindelig filvælger.
- Appen skal mindst understøtte JPEG, PNG og WebP, hvis den valgte billedbehandlingskomponent understøtter det på den valgte platform.
- Appen viser en forståelig fejl, hvis filen ikke kan læses, er ugyldig eller ikke understøttes.
- Appen viser originalbilledets bredde og højde.
- Appen må ikke ændre originalfilen. Output gemmes som nye filer.

### Formatvalg

- Formaterne grupperes efter platform og indholdstype.
- Brugeren kan vælge ét eller flere formater i samme arbejdsgang.
- Hvert format har en fast pixelstørrelse fra tabellen ovenfor; brugeren kan ikke ændre dimensionerne i MVP-versionen.

### Tilpasning og beskæring

- Outputbilleder skal have præcis de pixelmål, der er angivet for det valgte format.
- Brugeren kan vælge mellem:
  - **Tilpas hele billedet:** Hele billedet bevares inden for outputfladen, og eventuel ubrugt plads udfyldes med en valgt baggrundsfarve. Standard er hvid.
  - **Udfyld formatet:** Billedet skaleres, så outputfladen dækkes; overskydende områder beskæres.
- Ved beskæring vises en forhåndsvisning, så brugeren kan se, hvilke dele af billedet der udelades. Beskæringen centreres som standard.
- Ændring af tilpasningsvalg opdaterer forhåndsvisningen.
- Billedet skaleres med bevaret billedformat; det må ikke strækkes eller forvrænges.

### Eksport

- Brugeren kan gemme output som JPEG eller PNG.
- PNG bevarer gennemsigtighed, når det er muligt. JPEG-output flader gennemsigtighed ud mod den valgte baggrundsfarve.
- Brugeren kan vælge destinationsmappe og filnavnspræfiks.
- Appen opretter en særskilt fil for hvert valgt format med platform og indholdstype i filnavnet, eksempelvis `Facebook-Opslag-1200x1200.jpg`.
- Hvis en fil med samme navn allerede findes, skal appen spørge, om den skal overskrives, eller oprette et entydigt nyt filnavn. Eksisterende filer må ikke overskrives uden brugerens accept.
- Appen viser en tydelig status ved succes eller fejl og giver adgang til destinationsmappen efter eksport.

## Brugergrænseflade

- Hovedskærmen skal gøre det tydeligt, hvordan man vælger et billede, vælger outputformater og starter eksport.
- Forhåndsvisning og valgte dimensioner skal være synlige, før brugeren eksporterer.
- Fejlbeskeder skal forklare problemet og, hvor muligt, hvordan brugeren kan løse det.
- Appen skal fungere ved almindelige vinduesstørrelser og kunne betjenes med tastatur.

## Ikke-funktionelle krav

- Appen behandler billeder lokalt og sender ikke billedindhold eller filnavne over netværket.
- Appen skal kunne håndtere almindelige fotos i høj opløsning uden at fryse brugergrænsefladen under eksport.
- Billedbehandling skal bevare billedets visuelle kvalitet så godt som muligt; JPEG-kvaliteten skal være egnet til deling på sociale medier.
- Midlertidige billeddata skal frigives efter brug, så gentagne eksportopgaver ikke medfører unødigt voksende hukommelsesforbrug.
- Tekster og fejlbeskeder i MVP-versionen skal være på dansk.

## Afgrænsning for MVP

- Ingen direkte publicering eller login til sociale medier.
- Ingen cloudlagring eller automatisk upload.
- Ingen billedredigering ud over skalering, baggrundsudfyldning og beskæring.
- Ingen brugerdefinerede pixelmål.
- Ingen håndtering af video, animation eller flersidede billedformater.
- Eksport af ét valgt kildebillede pr. arbejdsgang.

## Acceptkriterier

1. Brugeren kan vælge et understøttet billede fra computeren og se det i appen.
2. Alle ti formater i størrelsestabellen kan vælges.
3. Eksportfilerne har præcis de dimensioner, der hører til deres valgte formater.
4. Billedets proportioner bevares; appen strækker ikke billedet.
5. Brugeren kan vælge mellem at bevare hele billedet med baggrund og at udfylde formatet med beskæring.
6. Forhåndsvisningen afspejler valgt platform, størrelse og tilpasningsmetode før eksport.
7. Brugeren kan eksportere som JPEG eller PNG til en valgt mappe.
8. Eksisterende filer overskrives ikke uden brugerens accept.
9. Ugyldige eller ikke-understøttede filer giver en forståelig fejl uden at lukke appen.
10. Originalbilledet forbliver uændret, og billedbehandlingen kræver ikke netværksadgang.

## Åbne afklaringer

- Skal appen være en Windows-desktopapp, eller skal den også kunne køre på macOS/Linux?
- Hvilken UI-teknologi skal anvendes i C#/.NET?
- Skal brugeren kunne flytte beskæringsområdet manuelt, eller er centreret beskæring tilstrækkelig i første version?
- Skal standardformatet ved eksport være JPEG eller PNG?
