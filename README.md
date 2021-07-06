## Language

English | [简体中文](README.zh_CN.md)

## CardPool

System implementation for card pool type card draw games, with the ability to easily build card pools (can use custom item class), set the rarity of individual cards and auto generate probability or set probabilities individually.

## Principle

First of all, about the card pool build. Cards have a true probability (relative to the whole pool), and the user can set the rarity of the card or set its probability individually, and then the system automatically generates the true probability according to the setting, and finally generates the interval layout as the basis for subsequent card draws.

For example, if a card has a true probability of 25%, then it has a line range of 0-0.25, and when the generated random number is within that range, the card is considered to be drawn. 

Using NextDouble in Random class to get random double and has made the random seed thread-safe.

