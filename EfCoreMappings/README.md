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

```csharp
public class Samurai // Principal
{
    public Samurai()
    {
        SamuraiBattles = new List<SamuraiBattle>();
    }
    public int Id {get; set;}
    public string Name {get; set;}
    public List<SamuraiBattle> SamuraiBattles {get; set;}
    public SecretIdentity SecretIdentity {get; set;}
}
public class SecretIdentity // Dependent
{
    public int Id {get; set;}
    public string RealName {get; set;}
    public int SamuraiId {get; set;} // FK in EF Core Convention
}
```

`SecretIdentity` must always have a parent, `SamuraiId` is a non-nullable int.

To allow `SamuraiId` be null, put the `?` symbol after type. But it break the rules of one-to-one relationship.
```csharp
public class SecretIdentity // Dependent
{
    public int Id {get; set;}
    public string RealName {get; set;}
    public int? SamuraiId {get; set;} // can be null
}
```

For the other hand, if you desire enforce that `Samurai` must have a `SecretIdentity` you have to handle with this by your own, in your business logic.

For example, you can pass the `SecretName` by constructor:
```csharp
public class Samurai
{
    public Samurai()
    {
        SamuraiBattles = new List<SamuraiBattle>();
    }
    public Samurai(string publicName, string secretName):this ()
    {
        Name = publicName;
        SecretIdentity = new SecretIdentity { RealName = secretName };
    }
    public int Id {get; set;}
    public string Name {get; set;}
    public List<SamuraiBattle> SamuraiBattles {get; set;}
    public SecretIdentity SecretIdentity {get; set;}
}
```

In order to allow navigation to `Samurai` object from `SecretIdentity` declare a Sumurai property. EF Core you identify the FK and will load the Samurai properly.
```csharp
public class SecretIdentity // Dependent
{
    public int Id {get; set;}
    public string RealName {get; set;}
    public Samurai Samurai {get; set;}
    public int SamuraiId {get; set;} 
}
```

If you don't want to have the `SamuraiId` Foreign Key in the class, then you must have to explicitly declare the relationship in DbContext configuration. However this approach increases the complexity unnecessarily.

Stay aware that the Nullability of FK property affects EF behavior.

## Shadow Properties

* Define shadow properties in mpappings
* Populate shadow properties
* Use or retrieve them in queries

The ability to persist data not defined. An application can be "audit information about how and when data was stored".

Entity defined:
```csharp
public class Samurai
{
    public int Id {get; set;}
    public string Name {get; set;}
}
```

Entity reflected in database:
| Column Name | Data type |
| ----------- | --------- |
| Id | int |
| Name | nvarchar(MAX) |
| LastModified | datetime |

We don't wanna show the property `LastModified` to the client.

You can define the shadow property in the DbContex configuration:
```csharp
modelBuilder.Entity<Samurai>()
    .Property<DateTime>("LastModified");
```

You can populate data in:
```csharp
_context.Entry(samurai)
    .Property("LastModified").CurrentValue = DateTime.Now;
```

You can even query by EF.Property:
```csharp
_context.Samurais
        .OrderBy(s => EF.Property<DateTime>(s, "LastModified"));
```

A real example:
```csharp
// hided for brevity
private static void CreateSamurai()
{
    var samurai = new Samurai { Name="Ronin" };
    _context.Samurais.Add(samurai);
    var timestamp = DateTime.Now;
    _context.Entry(samurai).Property("Created").CurrentValue = timestamp;
    _context.Entry(samurai).Property("LastModified").CurrentValue = timestamp;
    _context.SaveChanges();
}
```

You can load an anonymous type that consist of the `Id`, `Name` and the `Create` date from the database.

```csharp
private static void RetrieveSamuraisCreatedInPastWeek()
{
    var oneWeekAgo = DateTime.Now.AddDays(-7);
    var newSamurais = _context.Samurais
                                .Where(s => EF.Property<DateTime>(s, "Created") >= oneWeekAgo)
                                .ToList();
    
    var samuraisCreated = _context.Samurais
                                .Where(s => EF.Property<DateTime>(s, "Created") >= oneWeekAgo)
                                .Select(s => new {s.Id, s.Name, Created=EF.Property<DateTime>(s,"Created")})
                                .ToList();
}
```

How to apply shadow properties to all defined entity data:
```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    modelBuilder.Entity(entityType.Name).Property<DateTime>("Created");
    modelBuilder.Entity(entityType.Name).Property<DateTime>("LastModified");
}
```

Making DbContext responsible to insert or update the shadow properties data in transactions:
```csharp
public override int SaveChanges()
{
    ChangeTracker.DetectChanges();
    var timestamp = DateTime.Now;
    foreach (var entry in ChangeTracker.Entries()
        .Where(e => e.State==EntityState.Added || e.State==EntityState.Modified))
    {
        entry.Property("LastModified").CurrentValue = timestamp;

        if (entry.State==EntityState.Added)
        {
            entry.Property("Created").CurrentValue = timestamp;
        }
    }
    return base.SaveChanges();
}
```

## Owned Types

* Benefits of the owned type mapping
* Create and map an owned type
* Retrieving and updating entities with owned type properties
* Learn workarounds to some current shortcomings
* Map a DDD value object as an owned type

Representation of custom type:
```csharp
public class PersonName
{
    public PersonName(string givenName, string surName)
    {
        SurName = surName;
        GivenName = givenName;
    }
    public string GivenName {get; set;}
    public string SurName {get; set;}
    public string FullName => $"{GivenName} {SurName}";
    public string FullNameReverse => $"{SurName}, {GivenName}";
}
```

Use of PersonName custom type in the business types
```csharp
public class Samurai
{
    public int Id {get; set;}
    public PersonName BetterName {get; set;} // sub type
}
public class Contact
{
    public int Id {get; set;}
    public PersonName BetterName {get; set;} // sub type
}
```

In a doc no-SQL database, this kind of entity is easy persisted. But in the related database can be difficult to map this kind of composition. However, EF Core already has a feature to address this problem and transform subtypes properties in plain flat properties of the parent.

Here is how to register owned type in model configuration:
```csharp
// hided for brevity
modelBuilder.Entity<Samurai>().OwnsOne(s => s.BetterName);
```

This will produce the properties in the parent entity:
```
"BetterName_GivenName",
"BetterName_SurName"
```

You can register a custom column name:
```csharp
modelBuilder.Entity<Samurai>().OwnsOne(s => s.BetterName).Property(b => b.GivenName).HasColumnName("GivenName");
modelBuilder.Entity<Samurai>().OwnsOne(s => s.BetterName).Property(b => b.SurName).HasColumnName("SurName");
```

Or you can split in two tables:
```csharp
modelBuilder.Entity<Samurai>().OwnsOne(s => s.BetterName).ToTable("BatterNames");
```

This way the EF Core will infer the relationship and you have to do nothing more in your queries.

For the other hand, you must declare a private constructor without parameters in order to allow the mapper component to map the subtype by reflection.
```csharp
public class PersonName
{
    public PersonName(string givenName, string surName)
    {
        SurName = surName;
        GivenName = givenName;
    }
    private PersonName() {} // only reflection will see this
    // properties hided for brevity
}
```

***

:warning: An error occur when saving a new Samurai with PersonName.

**Problem** - `ModelBuilder` understands that owned types are not entities. `Change Tracker` does not understand this!

How to solve this problem in the overridden method `SaveChanges`:
```csharp
//hided for brevity
foreach (var entry in ChangeTracker.Entries()
    .Where(e => (e.State==EntityState.Added || e.State==EntityState.Modified) && !e.Metadata.IsOwned()))
    {
        // hided for brevity
    }
// continue
```

***

:warning: Even when your business logic doesn't care if the subtypes are null, the `ChangeTracker` not allow that `owned` types be persisted with null values.

**Workaround**

In the Owned Type Class:
* Factory methods: Create() & Empty()
* Private Constructor
* IsEmpty Property

```csharp
// PersonName class
// surrounded code hided for brevity
public static PersonName Create(string givenName, string surName)
{
    return new PersonName(givenName, surName);
}
public static PersonName Empty()
{
    return new PersonName("", "");
}
private PersonName(string givenName, string surName)
{
    SurName = surName;
    GivenName = givenName;
}
public bool IsEmpty()
{
    return SurName == "" & GivenName == "";
}
```

In the overridden method `SavedChanges`:

```csharp
public override int SaveChanges()
{
    ChangeTracker.DetectChanges();
    var timestamp = DateTime.Now;
    foreach (var entry in ChangeTracker.Entries()
        .Where(e => (e.State==EntityState.Added || e.State==EntityState.Modified) && !e.Metadata.IsOwned()))
    {
        entry.Property("LastModified").CurrentValue = timestamp;

        if (entry.State==EntityState.Added)
        {
            entry.Property("Created").CurrentValue = timestamp;
        }
        // workaround
        if (entry.Entity is Samurai)
        {
            if (entry.Reference("BetterName").CurrentValue == null)
            {
                entry.Reference("BetterName").CurrentValue = PersonName.Empty();
            }
        }
    }
    return base.SaveChanges();
}
```

And if you desire more consistency, you can turn the PensonName null again when retrieve the data from the database:
```csharp
private static void FixUpNullBetterName()
{
    _context = new SamuraiContext();
    var samurai = _context.Samurais.FirstOrDefault(s => s.Name == "Chrisjen");
    if (samurai is null) { return; }
    if (samurai.BetterName.IsEmpty())
    {
        samurai.BetterName = null;
    }
}
```

***

:warning: EF Core does not understand *replacing* owned type properties

```csharp
private static void ReplaceBetterName()
{
    var samurai = _context.Samurais.FirstOrDefault();
    samurai.BetterName = PersonName.Create("Shohreh", "Aghdashloo");
    _context.SaveChanges();
}
```

What happens is the `PersonName` object will always exist, even if empty. And when trying to add a new one, the `ChangeTracker` that already aware of one object, will be confused about the new one.

You'll need to help EF Core understand owned type replacements:
```csharp
// workaround in SaveChanges method
if (entry.Entity is Samurai)
{
    if (entry.Reference("BetterName").CurrentValue == null)
    {
        entry.Reference("BetterName").CurrentValue = PersonName.Empty();
    }
    entry.Reference("BetterName").TargetEntry.State = entry.State; // set the samurai state to PersonName state
}
```

If the object is **untracked**, access the `ChangeTracker` by `Update` and the EF Core will easily handle the update.
```csharp
_context.Samurais.Update(samurai)
```

However, if the object is **tracked**, detach the `PersonName` entity from `Samurai` entity.
```csharp
_context.Entry(samurai)
    .Reference(s => s.BetterName)
    .TargetEntry.State = EntityState.Detached;

// Set the new property now
samurai.BetterName = PersonName.Create("A", "B");

// DbSet.Update
_context.Samurais.Update(samurai);

// Then SaveChanges
_context.SaveChanges() // here the overridden method will identify the change in samurai state and for consequence, the state of PersonName as well
```