using Hd2.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hd2.API.Data;

public class HdDbContext(DbContextOptions<HdDbContext> options) : DbContext(options)
{
    public DbSet<Person> People { get; init; }
    public DbSet<Patient> Patients { get; init; }
    public DbSet<Doctor> Doctors { get; init; }
    public DbSet<Donor> Donors { get; init; }
    public DbSet<Procedure> Procedures { get; init; }
    public DbSet<Complication> Complications { get; init; }
    public DbSet<Hospital> Hospitals { get; init; }
    public DbSet<Organ> Organs { get; init; }
    public DbSet<OrganType> OrganTypes { get; init; }
    public DbSet<Address> Addresses { get; init; }
    public DbSet<DoctorProcedure> DoctorProcedures { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IS A
        modelBuilder
            .Entity<Patient>()
            .HasBaseType<Person>();

        // IS A
        modelBuilder
            .Entity<Doctor>()
            .HasBaseType<Person>();

        // Enforce unique Pwz for Doctor
        modelBuilder
            .Entity<Doctor>()
            .HasIndex(d => d.Pwz)
            .IsUnique();

        // IS A
        modelBuilder
            .Entity<Donor>()
            .HasBaseType<Person>();

        // 1. Auto-incremented primary key for Procedure
        modelBuilder
            .Entity<Procedure>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        // 2. Auto-incremented primary key for Address
        modelBuilder
            .Entity<Address>()
            .Property(a => a.Id)
            .ValueGeneratedOnAdd();

        // 3. Auto-incremented primary key for Complication
        modelBuilder
            .Entity<Complication>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        // 4. Auto-incremented primary key for Hospital
        modelBuilder
            .Entity<Hospital>()
            .Property(h => h.Id)
            .ValueGeneratedOnAdd();

        // 5. Auto-incremented primary key for Organ
        modelBuilder
            .Entity<Organ>()
            .Property(o => o.Id)
            .ValueGeneratedOnAdd();

        // 6. Auto-incremented primary key for OrganType
        modelBuilder
            .Entity<OrganType>()
            .Property(ot => ot.Id)
            .ValueGeneratedOnAdd();

        // 7. Auto-incremented primary key for DoctorProcedure
        modelBuilder
            .Entity<DoctorProcedure>()
            .Property(dp => dp.Id)
            .ValueGeneratedOnAdd();

        // One-to-one relationship between Hospital and Address
        modelBuilder
            .Entity<Hospital>()
            .HasOne(h => h.Address)
            .WithOne(a => a.Hospital)
            .HasForeignKey<Hospital>(h => h.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        // One-to-many relationship between Address and Person
        modelBuilder
            .Entity<Address>()
            .HasMany(a => a.People)
            .WithOne(p => p.Address)
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        // Many doctors in one hospital
        modelBuilder
            .Entity<Hospital>()
            .HasMany(h => h.Doctors)
            .WithOne(d => d.Hospital)
            .HasForeignKey(d => d.HospitalId)
            .OnDelete(DeleteBehavior.NoAction);

        // Many procedures in one hospital
        modelBuilder
            .Entity<Hospital>()
            .HasMany(h => h.Procedures)
            .WithOne(p => p.Hospital)
            .HasForeignKey(p => p.HospitalId)
            .OnDelete(DeleteBehavior.NoAction);

        // Many complications in one procedure
        modelBuilder
            .Entity<Procedure>()
            .HasMany(p => p.Complications)
            .WithOne(c => c.Procedure)
            .HasForeignKey(c => c.ProcedureId)
            .OnDelete(DeleteBehavior.NoAction);

        // One patient to many procedures
        modelBuilder
            .Entity<Patient>()
            .HasMany(p => p.Procedures)
            .WithOne(p => p.Patient)
            .HasForeignKey(p => p.PatientPesel)
            .OnDelete(DeleteBehavior.NoAction);

        // One organ in one procedure
        modelBuilder
            .Entity<Procedure>()
            .HasOne(p => p.Organ)
            .WithOne(o => o.Procedure)
            .HasForeignKey<Procedure>(p => p.OrganId)
            .OnDelete(DeleteBehavior.NoAction);

        // One donor to many organs
        modelBuilder
            .Entity<Donor>()
            .HasMany(d => d.Organs)
            .WithOne(o => o.Donor)
            .HasForeignKey(o => o.DonorPesel)
            .OnDelete(DeleteBehavior.NoAction);

        // One organ type to many organs
        modelBuilder
            .Entity<OrganType>()
            .HasMany(ot => ot.Organs)
            .WithOne(o => o.OrganType)
            .HasForeignKey(o => o.OrganTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Many doctors to many procedures
        modelBuilder
            .Entity<DoctorProcedure>()
            .HasOne(dp => dp.Doctor)
            .WithMany(d => d.DoctorProcedures)
            .HasForeignKey(dp => dp.DoctorPesel)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DoctorProcedure>()
            .HasOne(dp => dp.Procedure)
            .WithMany(p => p.DoctorProcedures)
            .HasForeignKey(dp => dp.ProcedureId)
            .OnDelete(DeleteBehavior.NoAction);


        base.OnModelCreating(modelBuilder);
    }
}