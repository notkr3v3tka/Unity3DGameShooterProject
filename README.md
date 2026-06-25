# Unity 3D Wave-Shooter

Ein rundenbasierter 3D Low-Poly Überlebens-Shooter, entwickelt mit Unity und C#. Kämpfe gegen immer stärker werdende Goblin-Wellen, wähle nach jeder Runde ein Upgrade und teste deine Ausdauer.

---

## Team

| Name | Aufgabenbereich |
|------|-----------------|
| Younes El Haddoury | Projektdokumentation, Präsentation, Szenen-Management, Minimap-Integration,  |
| Stanislav | 3D-Level-Design und Arena-Aufbau, UI-Integration (HUD, Menüs, Game Over Screen), Sound-Integration und Testing, Allgemeine Bugfixes und Feinschliff, Szenen-Management und Strukturierung|
| Nikita | Gameplay-Programmierung und Core Systems (Wave-System, Enemy AI), Kampfmechaniken (Melee-System, Stun, Knockback, Lifesteal), UpgradeCards-System (Kartenlogik, Kategorien, Balancing), Waffen- und Projektilsystem (Bullet Physics, Gun-System) |

**Datum:** 28. März 2026  
**Versionskontrolle:** Git (git.bib.de)

---

## Spielübersicht

Der Spieler startet in einer geschlossenen Low-Poly Arena und kämpft sich durch eskalierend schwierigere Goblin-Wellen. Nach jeder überstandenen Welle öffnet sich ein kartenbasiertes Upgrade-Menü, aus dem eine von vier Kategorien gewählt werden kann.

**Spielschleife:** Welle überleben → Alle Gegner eliminieren → Upgrade wählen → Wiederholen

---

## Features

**Kampfsystem**
- Ego-Perspektiven-Schusswaffe mit physikbasierter Projektilmechanik
- Nahkampfangriff (Melee Slash) mit präziser Hitbox
- Goblins mit 1000 HP Basiswert, NavMesh-Wegfindung und eigenem Angriffssystem

**Upgrade-System**
Vier Kategorien stehen zwischen den Wellen zur Auswahl:
- `Melee` — Nahkampfschaden und Reichweite erhöhen
- `Gun` — Feuerrate, Schaden oder Nachladegeschwindigkeit verbessern
- `Movement` — Bewegungsgeschwindigkeit und Agilität steigern
- `Survival` — Maximale HP erhöhen oder Regeneration freischalten

**UI & HUD**
- Live HP-Anzeige (Herzleiste und numerischer Wert), Wellenzähler, Kill-Tracker
- Minimap mit Echtzeit-Arenaübersicht
- Pause-Menü mit Mausempfindlichkeit-Einstellung
- Game-Over-Bildschirm mit Neustart-Option

**Audio**
- Soundeffekte für Schüsse, Treffer und wichtige Spielereignisse

---

## Aufbau & Montagereihenfolge

> Diese Reihenfolge beim Einrichten der Szene in Unity einhalten, um Referenzfehler zu vermeiden.

1. Map und Block-Prefabs in die Szene einfügen
2. PlayerController und Hauptkamera platzieren
3. GameLogicPrefab hinzufügen und CardScript zuweisen
4. UI Canvas einbauen (HUD, Minimap, Menüs)
5. Alle Skript-Referenzen im Inspector verknüpfen

---

## Architektur

Das Spiel läuft über Unitys `Update()`-Loop mit drei zentralen Verantwortlichkeiten:

**Eingabe** — Maus- und Tastatureingaben für Bewegung, Schießen und Nahkampf werden erfasst  
**Spielzustand** — automatische Pause bei geöffnetem Menü oder Upgrade-Panel  
**Wellenlogik** — Kill-Zähler wird getrackt, Schaden berechnet (`HP = HP - Damage`) und die nächste Welle bei Erreichen des Ziels ausgelöst

---

## Performance

Low-Poly Assets sorgen für hohe Bildwiederholraten auch bei vielen gleichzeitigen Gegnern. Alle Gegner und Projektile werden über Prefabs instanziiert, um Speicherlecks und unnötige Garbage Collection zu vermeiden.

---

## Geplante Verbesserungen

- Fernkämpfer und Boss-Gegner in höheren Wellen
- Weitere Waffenmodelle (Shotgun, Sniper usw.)
- Mehr Partikeleffekte für Treffer und Gegner-Tod
- Feinere Schwierigkeitsskalierung pro Welle (HP-Multiplikator, Spawn-Rate)
- Zusätzliche Arenen oder prozedurale Kartengenerierung

---

## Technologien

- **Unity 3D** mit C#
- **NavMesh** für Gegner Wegfindung
- **RenderTexture** für die Minimap
- **Git** zur Versionskontrolle (git.bib.de)