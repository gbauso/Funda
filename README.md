# Funda Assignment

### Goal
(makelaar = real estate agent)
(tuin = garden)

Determine which makelaar's  in Amsterdam have the most object listed for sale. Make a table of the top 10. 
Then do the same thing but only for objects with a tuin which are listed for sale.

### Tech

For this assignment, it was used:

- C#
- SQLite (For temporary table)
- Redis (Caching and avoid call the API all the time)
- 3rd party libraries for .NET

### Run it

This assignment requires Redis (Could be run in Docker) and .NET 5 to run

In case of needing to change some configuration, there is a file called appsettings.json on Funda.GUI project.

```sh
$ dotnet test
$ dotnet restore
$ docker run -p 6379:6379 -d redis
$ cd src/Funda.GUI
$ dotnet run [-g] [-f]
```

##### Arguments

```sh
  -g, --garden    Set output to top 10 sellers of houses containing garden.
  -f, --force     Force fetching and overwriting cache entry.
```

##### Output Example

```sh
-------------------------------------------------------------------
|Position|                                            Seller|  Ads|
-------------------------------------------------------------------
|       1|                           Eefje Voogd Makelaardij|  249|
-------------------------------------------------------------------
|       2|                              Broersma Makelaardij|   98|
-------------------------------------------------------------------
|       3|                 Hallie & Van Klooster Makelaardij|   95|
-------------------------------------------------------------------
|       4|                Ramón Mossel Makelaardij o.g. B.V.|   78|
-------------------------------------------------------------------
|       5|                Hoekstra en van Eck Amsterdam West|   76|
-------------------------------------------------------------------
|       6|              Makelaardij Van der Linden Amsterdam|   74|
-------------------------------------------------------------------
|       7|                        De Graaf & Groot Makelaars|   66|
-------------------------------------------------------------------
|       8|               Hoekstra en van Eck Amsterdam Noord|   64|
-------------------------------------------------------------------
|       9|          Smit & Heinen Makelaars en Taxateurs o/z|   64|
-------------------------------------------------------------------
|      10|                                  Heeren Makelaars|   63|
-------------------------------------------------------------------
```

### Things to improve

[ ] Write MORE Tests
[ ] Decouple caching from Repository
[ ] Make the Repository more resilient
[ ] Batch API Calls (Probably with parallelism)
[ ] Write a mechanism to limit HTTP calls per minute and use Retry implementation wisely.