using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Entities
{
    public static class LibraryContextExtensions
    {
        public static void EnsureSeedDataContext(this LibraryContext context)
        {
            // first, clear the database. This ensures we can always start
            // fresh with each demo. Not advised for production environment, obviously :-)

            context.Authors.RemoveRange(context.Authors);
            context.SaveChanges();

            // init seed data
            var authors = new List<Author>()
            {
                new Author()
                {
                    Id = new Guid(),
                    FirstName = "Stephen",
                    LastName = "King",
                    Genre = "Horror",
                    DateOfBirth = new DateTimeOffset(new DateTime(1947, 9, 21)),
                    Books = new List<Book>()
                    {
                        new Book()
                        {
                            Id = new Guid(),
                            Title = "The Shining",
                            Description = "The Shining is a horror novel by American author Stephen King."
                        }
                    }
                },
                new Author()
                {
                    Id = new Guid(),
                    FirstName = "Neil",
                    LastName = "Gaiman",
                    Genre = "Fantasy",
                    DateOfBirth = new DateTimeOffset(new DateTime(1960, 11, 10)),
                    Books = new List<Book>()
                },
                new Author()
                {
                    Id = new Guid(),
                    FirstName = "Tom",
                    LastName = "Lanoye",
                    Genre = "Various",
                    DateOfBirth = new DateTimeOffset(new DateTime(1958, 08, 27)),
                    Books = new List<Book>()
                }
            };

            context.Authors.AddRange(authors);
            context.SaveChanges();
        }
    }
}
