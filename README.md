# LibNXboxNROHelper

A tool that helps you download and process Xbox Game Pass game images with a custom overlay. Perfect for creating custom artwork for your game library!

## What Does This Tool Do?

This application allows you to:
- Search for Xbox Game Pass games by name
- Download official game tile images
- Apply a custom overlay to the images
- Save processed images with the game's xCloud ID

## Before You Start

### What You'll Need

1. **Xbox/Microsoft Account** - You'll need to sign in with your Microsoft account when the tool starts
2. **Overlay Image** (optional) - Place a file named `overlay.png` in the same folder as the application if you want to add a custom overlay to your images

## How to Use

### Step 1: Launch the Application

Double-click the application to start it. You'll be prompted to sign in with your Microsoft account.

### Step 2: Search for Games

1. When prompted with `Enter title name...`, type the name of a game you want to find
2. The tool will show you all matching games with their IDs
3. Continue entering more game names to add them to your list
4. When you're done searching, press **Enter** without typing anything to continue

**Example:**

### Step 3: Processing

The tool will automatically:
- Download each game's tile image
- Resize it to 242x242 pixels
- Apply your overlay (if provided)
- Save the processed image

### Step 4: Find Your Images

Once complete, you'll find your processed images in the `ProcessedImages` folder, located in the same directory as the application.

Each image will be named with the game's xCloud ID (e.g., `123456789.png`).

## Output Files

The tool creates the following:

- **ProcessedImages folder** - Contains all your processed game images
- **titles.txt** - A text file listing all processed games with their names and xCloud IDs

## Tips

- **Custom Overlay**: Your `overlay.png` file will be automatically resized to 242x242 pixels and placed on top of each game image
- **No Overlay**: If you don't have an overlay file, the tool will still work and save the original game images (resized)
- **Duplicates**: The tool automatically skips duplicate games, so don't worry about searching for the same game twice

## Troubleshooting

**"Warning: Overlay not found"**
- This is just a warning. The tool will continue without an overlay. Place `overlay.png` in the application folder if you want to use one.

**Image download fails**
- Some games may not have images available. The tool will display an error message but continue processing other games.

**Authentication issues**
- Make sure you're signed in with a valid Microsoft account
- Check your internet connection
