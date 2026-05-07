hey welcome

run dotnet run inside the ML_MP2 directory to execute the agent, and follow instructions in console.

oh yeah by the way i misread the instructions
and didnt recognize the packages were python,
so i'm using the wrong version and hitting rate limits.
i did implement exponential backoff, meaning you will be waiting like
64 seconds if it hits the limit continously.

i havnt allowed user input in the search, because it's easier to test with harcoded queries,
but it'll just be a while(true) and a console.readline() to change it.

good luck
