#region Copyright (c) 2000-2022 Sultan CRM BV

// ==========================================================
// 
// EfPlayground project - Copyright (c) 2000-2022 Sultan CRM BV
// ALL RIGHTS RESERVED
// 
// The entire contents of this file is protected by Dutch and
// International Copyright Laws. Unauthorized reproduction,
// reverse-engineering, and distribution of all or any portion of
// the code contained in this file is strictly prohibited and may
// result in severe civil and criminal penalties and will be
// prosecuted to the maximum extent possible under the law.
// 
// RESTRICTIONS
// 
// THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES
// ARE CONFIDENTIAL AND PROPRIETARY TRADE
// SECRETS OF SULTAN CRM BV. THE REGISTERED DEVELOPER IS
// NOT LICENSED TO DISTRIBUTE THE PRODUCT AND ALL ACCOMPANYING
// CODE AS PART OF AN EXECUTABLE PROGRAM.
// 
// THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED
// FILES OR ANY PORTION OF ITS CONTENTS SHALL AT NO TIME BE
// COPIED, TRANSFERRED, SOLD, DISTRIBUTED, OR OTHERWISE MADE
// AVAILABLE TO OTHER INDIVIDUALS WITHOUT EXPRESS WRITTEN CONSENT
// AND PERMISSION FROM SULTAN CRM BV.
// 
// CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON
// ADDITIONAL RESTRICTIONS
// 
// ===========================================================

#endregion

#region usings

using Microsoft.EntityFrameworkCore;
using PublisherData;
using PublisherDomain;

#endregion

// Create AdHoc new DB with tables and columns
// using PubContext context = new PubContext();
// context.Database.EnsureCreated();

// GetAuthors();
// AddAuthor();
// GetAuthors();

// AddAuthorWithBooks();
// GetAuthorsWithBooks();

var _context = new PubContext();
// Or via DbContext OnConfiguring(...)
//_context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking
// use DbSet.AsTracking() for special queries that needs to be tracked 

// Important: A fundamental understanding of how tracking work will pay of in productivity

//QueryFilters();
//FindIt();
//AddMoreAuthors();
//SkipAndTakeAuthors();
//SortAuthors();
//QueryAggregate();
//CoordinatedRetrieveAndUpdate();
//InsertMultipleAuthors();


void GetAuthors() {
    using var context = new PubContext();
    var authorList = context.Authors.ToList();
    foreach (var author in authorList) {
        Console.WriteLine(author.FirstName + " " + author.LastName);
    }
}

void AddAuthor() {
    var author = new Author() { FirstName = "Jos", LastName = "Nijsen" };
    using var context = new PubContext();
    context.Authors.Add(author);
    context.SaveChanges();
}

void AddAuthorWithBooks() {
    var author = new Author() { FirstName = "Jean Paul", LastName = "Teunisse" };
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
        Console.WriteLine(author.FirstName + " " + author.LastName);
        foreach (var book in author.Books) {
            Console.WriteLine("*" + book.Title);
        }
    }
}

void QueryFilters() {
    var name = "Jos";
    // Check SQL Profiles for SQL-Syntax
    var authorsWithoutParams = _context.Authors.Where(w => w.FirstName == "Jos").ToList();
    // To prevent SQL injection all queries with variables will be parameterized...
    var authorsWithParams = _context.Authors.Where(w => w.FirstName == name).ToList();

    // EF.Functions.Like
    var likeOperations = _context.Authors.Where(w => EF.Functions.Like(w.FirstName, "%Paul%")).ToList(); // SQL Like(%Paul%)
    // Linq Contains
    var containsOperations = _context.Authors.Where(w => w.FirstName.Contains("Paul")).ToList(); // SQL Like(%Paul%)
}

void FindIt() {
    // If found in memory avoids unneeded database query
    var authorIdTwo = _context.Authors.Find(2);
}

void AddMoreAuthors() {
    _context.Authors.Add(new Author() { FirstName = "Jaco", LastName = "de Koning" });
    _context.Authors.Add(new Author() { FirstName = "Michel", LastName = "Teunisse" });
    _context.Authors.Add(new Author() { FirstName = "Marlou", LastName = "van Lent" });
    _context.Authors.Add(new Author() { FirstName = "Ruben", LastName = "van Gemeren" });
    _context.SaveChanges();
}

void SkipAndTakeAuthors() {
    var groupSize = 2;
    for (var i = 0; i < 5; i++) {
        var authors = _context.Authors.Skip(groupSize * i).Take(groupSize).ToList();
        Console.WriteLine($"Group {i}:");
        foreach (var author in authors) {
            Console.WriteLine($"  {author.FirstName} {author.LastName}");
        }
    }
}

void SortAuthors() {
    var authorsByLastName = _context.Authors
        .OrderBy(s => s.LastName)
        .ThenBy(s => s.FirstName)
        .ToList();

    authorsByLastName.ForEach(a => Console.WriteLine(a.LastName + ", " + a.FirstName));

    var authorsDescending = _context.Authors
        .OrderByDescending(s => s.LastName)
        .ThenByDescending(s => s.FirstName)
        .ToList();

    Console.WriteLine("**Descending Last and First**");
    authorsDescending.ForEach(a => Console.WriteLine(a.LastName + ", " + a.FirstName));
}

void QueryAggregate() {
    var author = _context.Authors
        // Optional OrderBy sample...
        .OrderByDescending(s => s.FirstName)
        .FirstOrDefault(w => w.LastName == "Teunisse");

    // Missing OrderBy and gives runtime error!
    var runTimeError = _context.Authors
        .LastOrDefault(w => w.LastName == "Teunisse");
}

void RetrieveAndUpdateMultipleAuthors() {
    var wrongLastnames = _context.Authors.Where(w => w.LastName == "Teunisse").ToList();
    foreach (var wrongOne in wrongLastnames) {
        wrongOne.LastName = "Theunisse";
    }

    _context.SaveChanges();

    var correctLastnames = _context.Authors.Where(w => w.LastName == "Theunisse").ToList();
    foreach (var correctOne in correctLastnames) {
        correctOne.LastName = "Teunisse";
    }

    Console.WriteLine("Before: " + _context.ChangeTracker.DebugView.ShortView);
    _context.ChangeTracker.DetectChanges();
    Console.WriteLine("After: " + _context.ChangeTracker.DebugView.ShortView);

    _context.SaveChanges();
}

void CoordinatedRetrieveAndUpdate() {
    var author = FindThatAuthor(3);
    // No tracking here for author cause it isn't tracked by the DbContext!
    if (author?.FirstName != "Jean Paul") return;

    author.FirstName = "JeePeeTee";
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
        new Author() { FirstName = "Ruth", LastName = "Ozeki" },
        new Author() { FirstName = "Sofia", LastName = "Segovia" },
        new Author() { FirstName = "Ursula L.", LastName = "LeGuin" },
        new Author() { FirstName = "Hugh", LastName = "Howey" },
        new Author() { FirstName = "Isabella", LastName = "Allende" }
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