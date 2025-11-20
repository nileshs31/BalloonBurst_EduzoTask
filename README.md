# BalloonBurst_EduzoTask

A fast-paced, accuracy and reflex training game developed in Unity for an assignment task for Eduzo.

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Practice Mode](#practice-mode)
- [Test Mode](#test-mode)
- [Balloon Spawning System](#balloon-spawning-system)
- [Balloon Behaviour](#balloon-behaviour)
- [Caterpillar Target System](#caterpillar-target-system)
- [Lives System](#lives-system)
- [Accuracy and Scoring](#accuracy-and-scoring)
- [End Screen and Star Rating](#end-screen-and-star-rating)
- [Audio System](#audio-system)
- [Assumptions](#assumptions)
- [Future Improvements](#future-improvements)

---

## Overview
BalloonBurst is a lightweight Unity game featuring two gameplay modes: a free practice mode and a structured test mode.  
The project demonstrates dynamic UI generation, tween-based animations, resolution-independent spawning, and real-time scoring.

---

## Features
- Dynamic grid generation
- Floating balloons with tweened movement
- Hit/miss tracking and accuracy scoring
- Lives represented as heart icons
- Caterpillar tail progress system
- Timer with slider feedback
- End screen with star evaluation
- DOTween-powered animations
- Persistent background audio
- Loading screen with progress bar

---

Documentation Link - https://docs.google.com/document/d/1HKpkbg0qIC0elFtqOtt6rXxNnUl-_apI3igLqOPkNUY/edit?usp=sharing

Working Video PC - https://drive.google.com/file/d/1iWUO-_n0jZ-Oxh6bjs_qdLdD6K3XBB3w/view?usp=drive_link

---

## Practice Mode
Practice Mode offers unlimited grid-based balloon popping with customizable rows and columns.  
Each balloon respawns instantly with randomized colors and effects.

---

## Test Mode
Test Mode evaluates performance under constraints such as:
- Timer countdown  
- Lives  
- Caterpillar tail progress  
- Balloon misses  
Test ends on success, timeout, or losing all lives.

---

## Balloon Spawning System
Uses UI reference points (top, bottom, left, right) to generate resolution-independent spawn boundaries.  
Horizontal lanes are evenly distributed with slight random jitter.

---

## Balloon Behaviour
Includes upward movement, horizontal sway, pop animations, miss logic, respawn behavior, and particle/sound feedback.

---

## Caterpillar Target System
Tail count is randomly selected per test.  
Each correct pop removes one tail segment until none remain.

---

## Lives System
Heart icons visually represent remaining lives, updating dynamically upon misses.

---

## Accuracy and Scoring
Accuracy is calculated as:
```
accuracy = (BurstCount / (BurstCount + MissCount)) * 100
```

---

## End Screen and Star Rating
Performance is evaluated based on accuracy, awarding 0–3 stars and displaying detailed results.

---

## Audio System
A DontDestroyOnLoad audio manager plays persistent background music across scenes.

---

## Assumptions
- DOTween is installed
- Balloon prefab contains required UI components
- Reference points are positioned correctly

---

## Future Improvements
- Difficulty scaling
- Powerups and special balloons
- UI themes
- Leaderboards
- Performance analytics


