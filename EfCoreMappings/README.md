# Entity Framework Core 2: Mappings

## Agenda

* Many-to-many relationships
* Onet-to-one relationships
* Shadow properties for database, not entities
* Owned types for complex types and value objects
* Database scalar functions
* Database views

## Many-to-many relationships

* Understand how to define a many-to-many that EF Core will comprehend
* Creating, retrieving and modifying data in many-to-many graphs
* Learn how change tracker and whether or not objects are in memory affect behavior

In database there are `Join Table`, for the other hands in EF Core there are `Join Entity`.

EF Core does not understand a simple reference yet, like bellow:

```csharp
public class Samurai
{
    public int Id {get; set;}
    public string Name {get; set;}
    public List<Battle> Battles {get; set;} // reference to Battle
}
public class Battle
{
    public int Id {get; set;}
    public string Name {get; set;}
    public List<Samurai> Samurais {get; set;} // reference to Samurai
}
```

To address this relationship you must have a third entity:

```csharp
public class SamuraiBattle
{
    public int SamuraiId {get; set;} // key is necessary to allow .NET Core infer the relationship
    public Samurai Samurai {get; set;} // the object is necessary to allow navigation between entities
    public int BattleId {get; set;}
    public Battle Battle {get; set;}
}
public class Samurai
{
    public Samurai()
    {
        SamuraiBattles = new List<SamuraiBattle>(); // best practice to not forget to instantiate a list
    }
    public int Id {get; set;}
    public string Name {get; set;}
    public List<SamuraiBattle> SamuraiBattles {get; set;}
}
public class Battle
{
    public Battle()
    {
        SamuraiBattles = new List<SamuraiBattle>(); // best practice to not forget to instantiate a list
    }
    public int Id {get; set;}
    public string Name {get; set;}
    public List<SamuraiBattle> SamuraiBattles {get; set;}
}
```

When configuring the relationship entity add SamuraiId and BattleId as a composed primary key:
```csharp
// OnModelCreating method hided
model.Builder.Entity<SamuraiBattle>() {
    .HashKey(s => new { s.SamuraiId, s.BattleId });
}
// Other entities configuration hided
```

### CRUD 

#### CREATE

Persisting a relationship of two objects that are already persisted in database without involve objects:

```csharp
private static void JoinBattleAndSamurai()
{
    //Kikuchiyo id is 1, Siege of Osaka id is 3
    var sbJoin = new SamuraiBattle { SamuraiId = 1, BattleId = 3 };
    _contex.Add(sbJoin);
    _contex.SaveChanges();
}
```

To persist using objects untracked, without overhaed the update, use a new method `Attach`. With this method the contex you operate the change just in the new object, in this case the Samurai.

```csharp
private static void EnlistSamuraiIntoABattleUntracked()
{
    Battle battle;
    using (var separateOperation = new SamuraiContext())
    {
        battle = separateOperation.Battles.Find(1);
    }
    battle.SamuraiBattles.Add(new SamuraiBattle { SamuraiId = 2 });
    _context.Battles.Attach(battle);
    _context.SaveChanges();
}
```

#### READ

To navigate to the grandchild entity is a pain with the controller algorithm bellow:

```csharp
private static void GetSamuraiWithBattles()
{
    var samuraiWithBattles = _contex.Samurais
        .Include(s => s.SamuraiBattles)
        .ThenInclude(sb => sb.Battle)
        .FirstOrDefault(s => s.Id == 1);
    var battle = samuraiWithBattles.SamuraiBattles.First().Battle;
    var allTheBattles = new List<Battle>();
    foreach(var samuraiBattle in samuraiWithBattles.SamuraiBattles)
    {
        allTheBattles.Add(samuraiBattle.Battle);
    }
}
```

To avoid this situation, add the logic into the model:

```csharp
public class Samurai
{
    public Samurai()
    {
        SamuraiBattles = new List<SamuraiBattle>(); // best practice to not forget to instantiate a list
    }
    public int Id {get; set;}
    public string Name {get; set;}
    public List<SamuraiBattle> SamuraiBattles {get; set;}
    public List<Battle> Battles()
    {
        var battles = new List<Battle>();
        foreach (var join in SamuraiBattles)
        {
            battles.Add(join.Battle);
        }
        return battles;
    }
}
```

With this you may want logic to determine if there are no battles or they just haven't been loaded yet.

#### UPDATE & DELETE

To remove the relationship is easy:

```csharp
private static void RemoveJoinBetweenSamuraiAndBattleSimple()
{
    var join = new SamuraiBattle { BattleId =1, SamuraiId = 8 };
    _context.Remove(join);
    _context.SaveChange();
}
```

You can remove from an object too:

```csharp
private static void RemoveBattleFromSamurai()
{
    //Goal: Remove join between Shichiroji(Id=3) and Battle of Okehazama (Id=1)
    var samurai = _context.Samurais.Include(s => s.SamuraiBattles)
                        .ThenInclude(sb => sb.Battle)
                    .SingleOrDefault(s => s.Id == 3);
    var sbToRemove = samurai.SamuraiBattles.SingleOrDefault(sb => sb.BattleId == 1);
    //samurai.SamuraiBattles.Remove(sbToRemove); //remove via List<T>
    _context.Remove(sbToRemove); //remove using DbContext
    _context.SaveChange();
}
```

As you see above, you can use two strategies to remove a relationship: using the Id of entities, or using objects in memory.

The second strategy however may come with side effects. I occur when you decide to delete via List, before to call `SaveChange` the relationship you still be present in the graph.

In order to udpate the entities you also can use two strategies: delete and insert a new entity, or update a property in an existent entity.

## One-to-one relationships

* Understand how EF Core infers one-to-one mappings
* Learn how to affect the mappings
* Adding the child entity in one-to-one
* Removing or replacing the child entity