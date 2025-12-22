# RimAI Game Information Context List

This document lists the game information gathered by RimAI to provide context to the language model.

## General State
- **Date**: Current game date (Quadrum, Year, etc.)
- **Weather**: Current weather label
- **Wealth**: Total colony wealth
- **Population**: Number of free colonists

## Resources
- **Food**: Total nutrition edible by humans
- **Medicine**: Count of Herbal and Industrial medicine
- **Silver**: Total silver count
- **Components**: Count of Industrial and Spacer components
- **Steel**: Total steel count
- **Wood**: Total wood log count

## Colonists
For each colonist:
- **Name**: Short name
- **Job**: Current job label
- **Mood**: Mood level (High, Low, Broken, etc.) and significant thoughts
- **Health**: Visible health conditions (sicknesses, injuries)
- **Skills**: Skills with level > 8
- **Traits**: All traits

## Power
- **Grid Status**: Estimations based on total production vs. consumption
- **Stored**: Total energy stored in batteries
- **Producing**: Total energy production rate

## Research
- **Current Project**: Name of the active research project
- **Progress**: Percentage completion

## Events
- **Recent Letters**: Labels of the last 10 letters received
- **Active Quests**: Ongoing or available quests with time remaining

## Threats
- **Active Threats**: Count and type of hostile pawns on the map
- **Game Conditions**: Active conditions like Toxic Fallout, Solar Flare, etc.
