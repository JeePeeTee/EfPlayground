﻿#region Copyright (c) 2000-2022 Sultan CRM BV

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
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.Update;
using PublisherDomain;

#endregion

namespace PublisherData;

public class PubContext : DbContext {
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //base.OnConfiguring(optionsBuilder);

        optionsBuilder
            .UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=PubDatabase;Trusted_Connection=True");
        // No tracking active to improve performance
        // Hack: https://learn.microsoft.com/en-us/ef/core/querying/tracking
        //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
    
    private SqlServerModificationCommandBatch CheckMe { get; set; }
}

