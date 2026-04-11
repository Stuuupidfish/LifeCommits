# LifeCommits
I really like the green contribution squares in GitHub and Leetcode. They motivate me so I built this desktop app that lets you contribute to any goal.
It's completely local so there is no login needed and it saves onto your computer.
<img width="877" height="284" alt="Screenshot 2026-04-10 230758" src="https://github.com/user-attachments/assets/ac10913d-0c2b-4e19-8e73-279cf3793084" />

I realize it's currently more like MVP as of now. The delete button is too obscure and clickable and has no double-checking, and I still need to implement streak counting, and the grid is somewhat broken when it comes to updating every day. Ill fix it one day...

## Features:
- Adding and deleting goals
- Custom colors for goal grids
- Contributions with the option to add notes to your contribution
- An overview grid that compounds contributions across all your goals into a singular grid

## For those who want to use it:
There is an executable file for Windows users in the releases section of the repo

## Tech stack
- C# / .NET
- Avalonia
- Github Copilot

The way I built this project was initially by writing only the C# classes that defined the core functionality. And because I hate UI work and am unfamiliar with MVVM, I let Copilot help me get started on that. I ended up letting it do most of UX work along with JSON serialization and deserialization. 

Maybe one day I'll take the time to thoroughly learn everything but to be honest this time i around I was more focused on the end product over the process cuz I needed something to motivate me to keep up good habits.

The program is quite simple, but I hope that it could be of use to someone too 
