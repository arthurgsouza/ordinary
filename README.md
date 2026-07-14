# Ordinary Leaf

*A swipe-based narrative game about sustainability, agency, and the weight of ordinary choices, told from a Global South perspective.*

You play Leaf, a fledgling AI advisor the United Nations has just switched on for the first time. To earn the right to go live, you have to help thirteen people carry problems that have no clean answer: a farmer weighing this season's wages against next season's soil, a city deciding what it can afford to build. You answer the only way Leaf can, by swiping a card left or right. Every swipe quietly moves the balance of the planet, and you never see the numbers doing the moving.

The game was built as the practical half of an MSc dissertation at Manchester Metropolitan University (2025).

## Play it

Ordinary Leaf is published as a WebGL build on Unity Play, so it runs in the browser with nothing to install. It is designed portrait-first: on a phone you swipe, on a desktop you click and drag. (Add the Unity Play link here before sharing.)

## Running it locally

This repository uses Git LFS for its binary assets (art, audio, models), so install the LFS hooks once before you clone, or your clone will hold pointer files where the real assets should be:

```bash
git lfs install
git clone <your-repository-url>
```

Then open the project folder in Unity Hub with Unity 6000.5.2f1 (Unity 6.5), or the closest 6.5 release you have installed. Unity imports the project on first open, which takes a while. The playable scene is in `Assets/Scenes`.

## Credits

Designed and developed by Arthur Grangeiro de Souza, as part of an MSc dissertation at Manchester Metropolitan University (2025). Character art generated with DeepSeek R1 and Leonardo AI (Phoenix 1.0). Built with Unity and the Universal Render Pipeline.

## License

All rights reserved. This repository is private and not licensed for reuse, distribution, or modification. If it is opened up later, a proper license will be added here (the code and the AI-generated assets are separate decisions worth making deliberately).
