# Glossary

Intensity
- The amount an effect applies. Used to indicate stack amount, damage, and healing.

IntensityType
- The kind of Intensity to apply. Determines the utilization of the Intensity value.

Foe
- The opponent of the user of an effect.
- If the user of the effect is the Player, perhaps from a Card, this is a random enemy.
- If the user of the effect is any enemy, this is the Player.

# Scripting Tokens:

[SETTARGET: SELF]
- SETTARGET is the primary way to target something.
- Valid values are contextual.
- - SELF -> Target the user of the ability.
- - ORIGINAL -> Target the original target of the ability.
- - FOE -> Target a random valid foe. Targets the original if the original is a foe of the owner.

[DAMAGE: 3]
- Sets IntensityType to Damage, Intensity to 3.
- This will deal 3 damage to a target.
- If there is no token describing targeting, this targets the "foe".

[HEAL: 3]
- Sets IntensityType to Heal, Intensity to 3.
- This will heal a target for 3.
- If there is no token describing targeting, this targets the user.

[DRAW: 3]
- Sets IntensityType to NumberOfCards, Intensity to 3.
- This will draw 3 cards from the deck.
- This can only affect the player.

[GAINELEMENT: 3 CYBER]
- The player gains 3 CYBER
- This can be anywhere in the card, but will only gain you the element if the effect executes

[REQUIRESATLEASTELEMENT: 3 CYBER]
- The rest of the card, until the next section with its own explicit requirements, will require 3 Cyber Element in the player's pool to use.
- In the script "[DAMAGE: 1][REQUIRESATLESTELEMENT: 2 BIO][DAMAGE: 2][REQUIRESATLEASTELEMENT: 3 VOID][DAMAGE: 3]"
> [DAMAGE: 1] will always execute, [DAMAGE: 2] will only execute if the player has at least 2 Bio, and separately [DAMAEG: 3] will only
> execute if the player has 3 VOID.
- One section can have multiple requirements. "[REQUIRESATLEASTELEMENT: 2 BIO][REQUIRESATLEASTELMENT: 2 SOLAR][DAMAGE: 3]" will require both elements.
- If you have a requirement in a section, all following sections are assumed to have the same requirements until one is specified.
> In the script "[REQUIREATLEASTELEMENT: 2 BIO][DAMAGE: 1][SETTARGETSELF][HEAL: 1]", all of the effects will require 2 BIO. Heal 1 will only happen with the same conditions.
- If a player can't meet any of the requirements for any of the abilities of a card, they cannot play it.

[DRAINSELEMENT: 3 CYBER]
- This will require the user to have 3 Cyber to play this card. Effectively it is assumed that [REQUIRESATLEASTELEMENT: 3 CYBER] is also specified.
> Therefore you don't have to specify it in addition, you can juse just this.
- When played, it will reduce the player's Cyber element amount by 3.