#region MIT License

// ==========================================================
// 
// EfPlayground project - Copyright (c) 2022 JeePeeTee
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// ===========================================================

#endregion

#region usings

using Microsoft.EntityFrameworkCore;
using PublisherData;
using PublisherDomain;

#endregion

// Create AdHoc new DB with tables and columns
//using PubContext context = new PubContext();
//context.Database.EnsureCreated();

var _context = new PubContext();
_context.Database.EnsureDeleted();
_context.Database.EnsureCreated();

AddAuthorJeanPaul();
AddAuthorJos();
GetAuthors();

AddAuthorWithBooks();
GetAuthorsWithBooks();

GetAuthors();

// Or via DbContext OnConfiguring(...)
//_context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking
// use DbSet.AsTracking() for special queries that needs to be tracked 

// Important: A fundamental understanding of how tracking work will pay of in productivity

QueryFilters();
FindIt();
AddMoreAuthors();
SkipAndTakeAuthors();
SortAuthors();
QueryAggregate();
CoordinatedRetrieveAndUpdate();
InsertMultipleAuthors();


void GetAuthors() {
    //var authors = _context.Authors.ToList();

    // Loging active now
    // Parameters are by default protected for logging
    // See: EnableSensitiveDataLogging
    var name = "JeePeeTee";
    var authors = _context.Authors.Where(w => w.Name.First == name).ToList();

    // using var context = new PubContext();
    // var authorList = context.Authors.ToList();
    // foreach (var author in authorList) {
    //     Console.WriteLine(author.FirstName + " " + author.LastName);
    // }
}

void AddAuthorJeanPaul() {
    var author = new Author() { Name = { First = "Jean Paul", Last = "Teunisse" } };
    using var context = new PubContext();
    context.Authors.Add(author);
    context.SaveChanges();
}

void AddAuthorJos() {
    var author = new Author() { Name = { First = "Jos", Last = "Nijsen" } };
    using var context = new PubContext();
    context.Authors.Add(author);
    context.SaveChanges();
}

void AddAuthorWithBooks() {
    var author = new Author() { Name = { First = "Jean Paul", Last = "Teunisse" } };
    author.Books.Add(new Book() { Title = "My 1st Book", PublishDate = new DateTime(2009, 1, 1) });
    author.Books.Add(new Book() { Title = "My 2nd Book", PublishDate = new DateTime(2011, 6, 1) });
    using var context = new PubContext();
    context.Authors.Add(author);
    context.SaveChanges();
}

void GetAuthorsWithBooks() {
    using var context = new PubContext();
    var authors = context.Authors.Include(a => a.Books).ToList();
    foreach (var author in authors) {
        Console.WriteLine(author.Name.First + " " + author.Name.Last);
        foreach (var book in author.Books) {
            Console.WriteLine("*" + book.Title);
        }
    }
}

void QueryFilters() {
    var name = "Jos";
    // Check SQL Profiles for SQL-Syntax
    var authorsWithoutParams = _context.Authors.Where(w => w.Name.First == "Jos").ToList();
    // To prevent SQL injection all queries with variables will be parameterized...
    var authorsWithParams = _context.Authors.Where(w => w.Name.First == name).ToList();

    // EF.Functions.Like
    var likeOperations = _context.Authors.Where(w => EF.Functions.Like(w.Name.First, "%Paul%")).ToList(); // SQL Like(%Paul%)
    // Linq Contains
    var containsOperations = _context.Authors.Where(w => w.Name.First.Contains("Paul")).ToList(); // SQL Like(%Paul%)
}

void FindIt() {
    // If found in memory avoids unneeded database query
    var authorIdTwo = _context.Authors.Find(2);
}

void AddMoreAuthors() {
    _context.Authors.Add(new Author() { Name = { First = "Jaco", Last = "de Koning" } });
    _context.Authors.Add(new Author() { Name = { First = "Michel", Last = "Teunisse" } });
    _context.Authors.Add(new Author() { Name = { First = "Marlou", Last = "van Lent" } });
    _context.Authors.Add(new Author() { Name = { First = "Ruben", Last = "van Gemeren" } });
    _context.SaveChanges();
}

void SkipAndTakeAuthors() {
    var groupSize = 2;
    for (var i = 0; i < 5; i++) {
        var authors = _context.Authors.Skip(groupSize * i).Take(groupSize).ToList();
        Console.WriteLine($"Group {i}:");
        foreach (var author in authors) {
            Console.WriteLine($"  {author.Name.Full}");
        }
    }
}

void SortAuthors() {
    var authorsByLastName = _context.Authors
        .OrderBy(s => s.Name.Last)
        .ThenBy(s => s.Name.First)
        .ToList();

    authorsByLastName.ForEach(a => Console.WriteLine(a.Name.Reverse));

    var authorsDescending = _context.Authors
        .OrderByDescending(s => s.Name.Last)
        .ThenByDescending(s => s.Name.First)
        .ToList();

    Console.WriteLine("**Descending Last and First**");
    authorsDescending.ForEach(a => Console.WriteLine(a.Name.Reverse));
}

void QueryAggregate() {
    var author = _context.Authors
        // Optional OrderBy sample...
        .OrderByDescending(s => s.Name.First)
        .FirstOrDefault(w => w.Name.Last == "Teunisse");

    // Missing OrderBy and gives runtime error!
    // var runTimeError = _context.Authors
    //     .LastOrDefault(w => w.Name.Last == "Teunisse");
}

void RetrieveAndUpdateMultipleAuthors() {
    var wrongLastnames = _context.Authors.Where(w => w.Name.Last == "Teunisse").ToList();
    foreach (var wrongOne in wrongLastnames) {
        wrongOne.Name.Last = "Theunisse";
    }

    _context.SaveChanges();

    var correctLastnames = _context.Authors.Where(w => w.Name.Last == "Theunisse").ToList();
    foreach (var correctOne in correctLastnames) {
        correctOne.Name.Last = "Teunisse";
    }

    Console.WriteLine("Before: " + _context.ChangeTracker.DebugView.ShortView);
    _context.ChangeTracker.DetectChanges();
    Console.WriteLine("After: " + _context.ChangeTracker.DebugView.ShortView);

    _context.SaveChanges();
}

void CoordinatedRetrieveAndUpdate() {
    var author = FindThatAuthor(3);
    // No tracking here for author cause it isn't tracked by the DbContext!
    if (author?.Name.First != "Jean Paul") return;

    author.Name.First = "JeePeeTee";
    // All fields will be updated!
    SaveThatAuthor(author);
}

Author? FindThatAuthor(int authorId) {
    using var shortLivedContext = new PubContext();
    return shortLivedContext.Authors.Find(authorId);
}

void SaveThatAuthor(Author author) {
    using var anotherShortLivedContext = new PubContext();
    anotherShortLivedContext.Authors.Update(author);
    anotherShortLivedContext.SaveChanges();
}

void InsertMultipleAuthors() {
    var authorList = new Author[] {
        new Author() { Name = { First = "Ruth", Last = "Ozeki" } },
        new Author() { Name = { First = "Sofia", Last = "Segovia" } },
        new Author() { Name = { First = "Ursula L.", Last = "LeGuin" } },
        new Author() { Name = { First = "Hugh", Last = "Howey" } },
        new Author() { Name = { First = "Isabella", Last = "Allende" } }
    };
    // Also contains UpdateRange(...) and RemoveRange(...)
    _context.Authors.AddRange(authorList);
    // Uses SQL Merge statement, also known as a Bulk operation
    // Faster to send individual commands for 1-3 entities
    // Fatser to send batched commands for 4+ entities

    var book = _context.Books.Find(2);
    book.Title = "A new book title here...";

    // The bulk operation will handle Insert/Query/Update all in once
    // Including multiple object types
    // See SQL profiler for results on this 
    _context.SaveChanges();

    // Check SqlServerModificationCommandBatch within PublisherData.PubContext for its defaults
    // It is possible to override them when configuring the provider. 
}