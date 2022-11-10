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

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.Extensions.Logging;
using PublisherDomain;

#endregion

namespace PublisherData;

public class PubContext : DbContext {
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    public DbSet<Audit> AuditLogs { get; set; }

    private SqlServerModificationCommandBatch CheckMe { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder
            .UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=PubDatabase;Trusted_Connection=True")
            // Hack: https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/simple-logging
            .LogTo(
                log => Debug.WriteLine(log),
                new[] { DbLoggerCategory.Database.Command.Name },
                LogLevel.Information)
            .EnableSensitiveDataLogging(Debugger.IsAttached);

        // No tracking active to improve performance
        // Hack: https://learn.microsoft.com/en-us/ef/core/querying/tracking
        //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);  
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Author>().OwnsOne(a => a.Name);
    }

    // ======================
    // Audit trail stufff....
    // ======================

    public virtual int SaveChanges(string userId = "Admin") {
        OnBeforeSaveChanges(userId);
        var result = base.SaveChanges();
        return result;
    }

    private void OnBeforeSaveChanges(string userId) {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        foreach (var entry in ChangeTracker.Entries()) {
            if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;
            var auditEntry = new AuditEntry(entry);
            auditEntry.TableName = entry.Entity.GetType().Name;
            auditEntry.UserId = userId;
            auditEntries.Add(auditEntry);
            foreach (var property in entry.Properties) {
                var propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey()) {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State) {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Delete;
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified) {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }

                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries) {
            AuditLogs.Add(auditEntry.ToAudit());
        }
    }
}