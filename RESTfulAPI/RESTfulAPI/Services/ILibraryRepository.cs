using RESTfulAPI.Entities;
using RESTfulAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTfulAPI.Services
{
    public interface ILibraryRepository
    {
        bool AuthorExists(Guid authorId);

        PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters);

        IEnumerable<Author> GetAuthors(IEnumerable<Guid> ids);

        Author GetAuthor(Guid authorId);

        void AddAuthor(Author author);

        void DeleteAuthor(Author author);

        void UpdateAuthor(Author author);

        IEnumerable<Book> GetBooksForAuthor(Guid authorId);

        Book GetBookForAuthor(Guid authorId, Guid bookId);

        void AddBookForAuthor(Guid authorId, Book book);

        void UpdateBookForAuthor(Book book);

        void DeleteBook(Book book);

        bool Save();
    }
}
