## Master Angewandte Informatik
Computer Games / Wintersemester 19/20
![Fish Are Friends](Doku/splash.jpg?raw=true "Splash-Screen")
Foto-Quelle: Getty Images/Last Resort


## 18.12.2019: Erster Mood-Test
[![Fish Are Friends](https://img.youtube.com/vi/WbI-Z4hy8Qo/0.jpg)](https://www.youtube.com/watch?v=WbI-Z4hy8Qo)
  - Mood-Test
    - erster Test für den Unterwasserlook
    - Schwebepartikel um Wasser und Raum anzudeuten
    - GodRays um Wasseroberfläche / Lichtbrechung zu imitieren
    - Shader-Graph Caustics am Meeresgrund um Lichtbrechung zu imitieren
    
---

## 19.12.2019: Flocking-Tests / Futtersuche-Tests
![Fish Are Friends](Doku/test_flocking_2.jpg?raw=true "Splash-Screen")
<img src="Doku/test_flocking_1.jpg?raw=true" width="433"/> <img src="Doku/test_food_0.jpg?raw=true" width="433"/>
  - Flocking-Tests
    - allgemeines Schwarmverhalten mit Kollisionsvermeidung
  
  - Futtersuche-Tests
    - KI findet automatisch das nahe gelegenste Futter

---

## 21.12.2019: Flocking-Simulation & Shader Graphs / Unterwasserwelt
![Fish Are Friends](Doku/undewater_flocking_fishes.jpg?raw=true "Underwater Scene")

#### Flocking-Simulation
  - allgemeines Schwarmverhalten
  - Kollisionsvermeidung
  - Fluchtverhalten beim Anwesenheit eines Feindes
  - Schwarm-Gruppierung die treu dem Spieler-Fisch folgt
  - Farbwechsel der zum eigenen Schwarm gehörenden Fische
    - Übernahme der Farben aus dem Editor
    - Manipulation des Shader-Graphs zur Laufzeit

#### Shader-Graphs / Optik der Unterwasserwelt
  - Shader-Graph
    - Fisch-"Texturierung" (Procedural)
    - Shader-basierte Schwimm-Animation
    - Dither-Effekt um Vertex-Clipping zu vermeiden
    - Shader-Graph mit Sub-Graphs um Wiederverwendbarkeit zu erhöhen
  - Procedural generierte Unterwasserwelt (Basic)
  - flüssiger Kamerawechsel: hinter dem Spieler / Top-View
  
 ---
 
 ## 22.12.2019: Grass-Planting, Food-Chasing & EcoSystemManager
![Fish Are Friends](Doku/food_chasing_1.jpg?raw=true "Underwater Scene")
#### Food-Chasing
  - Fische verfügen nun über ein Hungerbedürfnis
  - Hunger zu stillen ist wichtiger als der Gruppenzusammenhalt
    - ist die Nahrung nah genug, wird die Gruppe kurzzeitig verlassen
    - ist die Nahrung zu weit weg, ist der kleine Fisch dennoch hin und her:
      - Gruppe oder Essen: Ohne Entscheidung stirbt der kleine Freund :(
  - Nahrung wird nach Bedarf generiert
  - Größe der Nahrungsquelle repräsentiert die angebotene Nahrungsmenge
  - Nahrungsanfragen werden komplett oder zum Teil gedeckt (ja nach Vorrat)
  - Fische die nicht "satt" werden, suchen automatisch die nächste Quelle

![Fish Are Friends](Doku/plants_1.jpg?raw=true "Underwater Scene")
#### Grass-Planting
  - Grundlage für die weitere Generierung der Unterwasserwelt
  - bestimmen der Platzgebiete per Perlin-Noise-Generierung von Zonen / Bereichen
  - animiertes Grass per Shader-Graph (wogt sich sanft in der Strömung)

#### EcoSystemManager
  - zeigt Statistiken über das Unterwasser-Biotop
    - Anzahl der Fische
    - auch die Anzahl der bereits Verstorbenen :(
    - deren Nahrungsbedarf (wenn es nicht schon zu spät ist)
    - noch vorhandene Nahrung
  - dient dem Balancing der Überlebenschancen
  - kann automatisch Futter generieren wenn das Gleichgewicht gefährdet ist
  - beim der Entwicklung dieser Erweiterung, sind 3.453.871 Alpha-Tester gestorben
    - sie mögen in Frieden ruhen - blubb, blubb


