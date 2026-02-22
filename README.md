# CardRPG - C# Console Deckbuilder

## 🌟 About the Project
CardRPG is a turn-based strategy game where the player develops their character, manages attributes, and battles enemies using a deck of cards. The game emphasizes the synergy between character stats and card performance.

## 🗺️ Game Loop & Locations
The player can choose between three primary activities:
* **🛒 Shop:** Purchase items that permanently increase attributes (Strength, Agility, Intelligence, HP, Mana, Armor).
* **🍺 Tavern:** Restore Health Points (HP) by purchasing various food and drinks.
* **🧭 Journey:** An adventure mode divided into stages. Each stage concludes with a Boss fight. Victories reward the player with gold, items, or new cards.

## ⚔️ Combat Mechanics
- **Decks & Cards:** The player manages a pool of cards (e.g., 20 cards). In each turn, they choose 1 out of 3 cards drawn to their hand.
- **Cooldown System:** A used card is moved to a "cooldown queue" and becomes unavailable for the next 3 turns, forcing strategic rotation.
- **Attributes:**
    - **Strength:** Increases base damage dealt.
    - **Agility:** Increases dodge chance and protects against enemy critical hits.
    - **Intelligence:** Increases the player's critical hit chance.
- **Intents:** Players can see the enemy's planned action (e.g., attack, defend) before making their move.

## 🛠️ Tech Stack
- **Language:** C# 12+
- **Platform:** .NET 10
- **Paradigm:** Object-Oriented Programming (OOP)
- **Data Persistence:** JSON (System.Text.Json) for save files and item databases.
