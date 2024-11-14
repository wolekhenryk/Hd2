using System.Collections.Concurrent;
using System.Diagnostics;
using Bogus;
using EFCore.BulkExtensions;
using Hd2.API.Data;
using Hd2.API.Models;
using Microsoft.EntityFrameworkCore;
using Person = Hd2.API.Models.Person;

namespace Hd2.API.Services;

public record OrganDonor(Organ Organ, Donor Donor);

public class Generator(HdDbContext dbContext, ILogger<Generator> logger)
{
    private const int NumberOfHospitals = 2;

    private readonly Faker _faker = new();

    private readonly string[] _wojewodztwa =
    [
        "Dolnośląskie",
        "Kujawsko-Pomorskie",
        "Lubelskie",
        "Lubuskie",
        "Łódzkie",
        "Małopolskie",
        "Mazowieckie",
        "Opolskie",
        "Podkarpackie",
        "Podlaskie",
        "Pomorskie",
        "Śląskie",
        "Świętokrzyskie",
        "Warmińsko-Mazurskie",
        "Wielkopolskie",
        "Zachodniopomorskie"
    ];

    private readonly string[] _typyNarzadow =
    [
        "Wątroba",
        "Płuca",
        "Serce",
        "Nerka",
        "Trzustka",
        "Jelito",
        "Żołądek",
        "Mózg",
        "Ręka",
        "Noga",
        "Oko",
        "Ucho",
        "Nos",
        "Gardło",
        "Wargi",
        "Zęby",
        "Język",
        "Szyja",
        "Ramię"
    ];

    private readonly string[] _specjalizacje =
    [
        "Anestezjolog",
        "Chirurg",
    ];

    private readonly string[] _grupyKrwi =
    [
        "A+",
        "A-",
        "B+",
        "B-",
        "AB+",
        "AB-",
        "0+",
        "0-"
    ];

    private readonly string[] _typyPrzechowywaniaNarzadow =
    [
        "Lód",
        "Sól",
        "Alkohol",
        "Formalina",
    ];

    private readonly string[] _typyKomplikacji =
    [
        "Zakażenie",
        "Odrzut",
        "Zator",
        "zawał"
    ];

    public async Task GenerateT1(int days)
    {
        await GenerateHospitalsAndDoctors();

        await GenerateOrganTypes();

        await GenerateProcedures(days, false);

        await GenerateComplications();
    }

    public async Task GenerateT2(int days)
    {
        await GenerateProcedures(days, true);

        await GenerateComplications();

        await ChangeAddressOfAPatient();
    }

    private async Task ChangeAddressOfAPatient()
    {
        // Randomly select a patient
        var patient = await dbContext.Patients
            .Include(p => p.Address)
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefaultAsync();

        if (patient is null)
            throw new NullReferenceException("No patient found.");

        var newAddress = GenerateFakeAddress();

        // Add the new address to the database
        await dbContext.Addresses.AddAsync(newAddress);
        await dbContext.SaveChangesAsync();

        // Hold the old address reference
        var oldAddress = patient.Address;

        // Update patient's address to the new one
        patient.Address = newAddress;
        dbContext.Patients.Update(patient);
        await dbContext.SaveChangesAsync();
    }

    private async Task GenerateProcedures(int daysToGoBack, bool t2)
    {
        var hospitals = await dbContext
            .Hospitals
            .Include(h => h.Doctors)
            .ToListAsync();

        foreach (var hospital in hospitals)
        {
            var sw = Stopwatch.StartNew();

            var proceduresDaily = hospital.Doctors.Count / 2;
            var procedureCount = daysToGoBack * proceduresDaily;

            var patients = await GeneratePatients(procedureCount);

            await dbContext.BulkInsertAsync(patients);

            var procedureDates = Enumerable
                .Range(1, daysToGoBack)
                .Select(i => DateTime.Now.AddDays(t2 ? i : -i))
                .ToList();

            var donorsOrgansList = await GenerateDonorsAndOrgans(procedureCount, proceduresDaily, procedureDates);

            var patientBatches = patients
                .Chunk(proceduresDaily)
                .ToList();

            var donorsOrgansBatches = donorsOrgansList
                .OrderBy(o => o.Organ.ScheduledProcedureDate)
                .Chunk(proceduresDaily)
                .ToList();

            var doctorTeams = hospital.Doctors
                .Chunk(2)
                .ToList();

            var procedures = patientBatches
                .Zip(donorsOrgansBatches, (patientBatch, donorOrganBatch) => (patientBatch, donorOrganBatch))
                .Zip(procedureDates, (patientOrganDonor, procedureDate) => (
                    patientOrganDonor.patientBatch,
                    patientOrganDonor.donorOrganBatch,
                    procedureDate))
                .SelectMany(element =>
                {
                    var (patientBatch, donorsOrgans, date) = element;

                    return patientBatch
                        .Zip(doctorTeams, (patient, doctors) => (patient, doctors))
                        .Zip(donorsOrgans,
                            (patientDoctor, donorOrgan) => (patientDoctor.patient, patientDoctor.doctors, donorOrgan))
                        .Select(patientDoctorDonorOrgan =>
                        {
                            var (patient, doctors, donorOrgan) = patientDoctorDonorOrgan;

                            return GenerateProcedure(hospital.Id, date, patient.Pesel, donorOrgan.Organ.Id);
                        });
                })
                .ToList();

            await dbContext.Procedures.AddRangeAsync(procedures);
            await dbContext.SaveChangesAsync();

            List<DoctorProcedure> doctorProcedures = [];

            foreach (var dailyProcedures in procedures.GroupBy(p => p.StartDateTime.Date))
            {
                doctorProcedures.AddRange(dailyProcedures
                    .Zip(doctorTeams, (procedure, doctors) => (procedure, doctors))
                    .SelectMany(element =>
                    {
                        var (procedure, doctors) = element;

                        // Return 2 doctor procedures for each procedure
                        return doctors
                            .Select(doctor => new DoctorProcedure
                            {
                                DoctorPesel = doctor.Pesel,
                                ProcedureId = procedure.Id
                            });
                    }));
            }

            await dbContext.BulkInsertAsync(doctorProcedures);

            sw.Stop();

            logger.LogInformation($"Generated {procedures.Count} procedures in {sw.ElapsedMilliseconds} ms");
        }
    }

    private async Task GenerateComplications()
    {
        var procedures = await dbContext.Procedures.ToListAsync();

        // Get randomly 10% of procedures
        procedures = procedures
            .Where(_ => Random.Shared.NextDouble() < 0.1)
            .ToList();

        var complications = procedures
            .SelectMany(procedure =>
            {
                var complicationCount = Random.Shared.Next(0, 3);

                return Enumerable.Range(0, complicationCount)
                    .Select(_ => new Complication
                    {
                        Type = _faker.PickRandom(_typyKomplikacji),
                        Description = _faker.Lorem.Sentence(),
                        ProcedureId = procedure.Id
                    });
            })
            .ToList();

        await dbContext.BulkInsertAsync(complications);
    }

    private async Task<List<Patient>> GeneratePatients(int count)
    {
        var fakePeople = await GenerateFakePeople(count);

        return fakePeople
            .Select(person => new Patient
            {
                Pesel = person.Pesel,
                BloodType = _faker.PickRandom(_grupyKrwi),
                HealthStatus = _faker.Lorem.Sentence()
            })
            .ToList();
    }

    private async Task<List<OrganDonor>> GenerateDonorsAndOrgans(int count, int dailyProcedures,
        IEnumerable<DateTime> procedureDates)
    {
        var fakePeople = await GenerateFakePeople(count);

        var donors = fakePeople
            .Select(person => new Donor
            {
                Pesel = person.Pesel,
                BloodType = _faker.PickRandom(_grupyKrwi),
                AliveDuringExtraction = Random.Shared.NextDouble() < 0.99,
                HealthStatus = _faker.Lorem.Sentence()
            })
            .ToList();

        await dbContext.BulkInsertAsync(donors);

        // reload donors to get their ids

        var organTypes = await dbContext.OrganTypes.ToListAsync();

        List<Organ> organs = [];

        var batched = donors
            .Chunk(dailyProcedures)
            .Zip(procedureDates, (donorsBatch, dates) => (donorsBatch, dates));

        foreach (var (donorsBatch, date) in batched)
        {
            organs.AddRange(donorsBatch.Select(donor => new Organ
            {
                HarvestDateTime = date.AddDays(-Random.Shared.Next(1, 14)),
                ScheduledProcedureDate = date,
                StorageType = _faker.PickRandom(_typyPrzechowywaniaNarzadow),
                OrganTypeId = _faker.PickRandom(organTypes).Id,
                DonorPesel = donor.Pesel
            }));
        }

        await dbContext.Organs.AddRangeAsync(organs);
        await dbContext.SaveChangesAsync();

        return donors
            .Zip(organs, (donor, organ) => new OrganDonor(organ, donor))
            .ToList();
    }

    private Procedure GenerateProcedure(int hospitalId,
        DateTime today,
        string patientsPesel,
        int organId)
    {
        var minutes = Random.Shared.Next(0, 60);
        var seconds = Random.Shared.Next(0, 60);

        var randomDurationInMinutes = Random.Shared.Next(180, 360);
        var startDateTime = new DateTime(today.Year, today.Month, today.Day, 8, minutes, seconds);

        return new Procedure
        {
            // Procedure starts at 8:00
            StartDateTime = startDateTime,
            EndDateTime = startDateTime.AddMinutes(randomDurationInMinutes),
            PatientPesel = patientsPesel,
            HospitalId = hospitalId,
            OrganId = organId
        };
    }

    private async Task GenerateOrganTypes()
    {
        var organTypes = _typyNarzadow
            .Select(name => new OrganType { Name = name })
            .ToList();

        await dbContext.BulkInsertAsync(organTypes);
    }

    private async Task GenerateHospitalsAndDoctors()
    {
        var hospitalAddresses = Enumerable
            .Range(0, NumberOfHospitals)
            .Select(_ => GenerateFakeAddress())
            .ToList();

        await dbContext.BulkInsertAsync(hospitalAddresses);

        // reload addresses to get their ids
        hospitalAddresses = await dbContext.Addresses.ToListAsync();

        var hospitals = hospitalAddresses
            .Select(address => new Hospital
            {
                AddressId = address.Id,
                Name = _faker.Company.CompanyName()
            })
            .ToList();

        await dbContext.BulkInsertAsync(hospitals);

        // reload hospitals to get their ids
        hospitals = await dbContext.Hospitals.ToListAsync();

        var peopleBatches = new List<Person[]>();

        var peopleBatchSizes = hospitals
            .Select(_ => Random.Shared.Next(10, 15) * 2)
            .ToArray();

        var fakePeople = await GenerateFakePeople(peopleBatchSizes.Sum());

        var amountToSkip = 0;
        foreach (var peopleBatchSize in peopleBatchSizes)
        {
            var peopleBatch = fakePeople
                .Skip(amountToSkip)
                .Take(peopleBatchSize)
                .ToArray();

            peopleBatches.Add(peopleBatch);
            amountToSkip += peopleBatchSize;
        }

        ConcurrentBag<Doctor[]> doctorBatches = [];

        var stopwatch = Stopwatch.StartNew();

        hospitals
            .Zip(peopleBatches, (hospital, people) => (hospital, people))
            .AsParallel()
            .ForAll(pair =>
            {
                var (hospital, people) = pair;
                var threadFaker = new Faker();

                var doctors = GenerateDoctors(hospital.Id, threadFaker, people);
                doctorBatches.Add(doctors);
            });

        stopwatch.Stop();

        var allDoctors = doctorBatches.SelectMany(doctors => doctors).ToArray();

        logger.LogInformation($"Generated {allDoctors.Length} doctors in {stopwatch.ElapsedMilliseconds} ms");

        await dbContext.BulkInsertAsync(allDoctors);
    }

    private Doctor[] GenerateDoctors(int hospitalId, Faker faker, IEnumerable<Person> people)
    {
        var doctors = people
            .Select(person => new Doctor
            {
                Pesel = person.Pesel,
                HospitalId = hospitalId,
                Specialization = faker.PickRandom(_specjalizacje),
                Pwz = Guid.NewGuid().ToString("N"),
            })
            .ToArray();

        return doctors;
    }

    private async Task<List<Person>> GenerateFakePeople(int count)
    {
        var peselCounter = dbContext.People.Count();

        var randomAddresses = Enumerable.Range(0, count)
            .Select(_ => GenerateFakeAddress())
            .ToList();

        await dbContext.BulkInsertAsync(randomAddresses);

        var lastAddressId = dbContext.Addresses.Max(a => a.Id);

        var i = 0;

        var fakePeopleGenerator = new Faker<Person>()
            .CustomInstantiator(f =>
            {
                var pesel = peselCounter.ToString("D11");

                peselCounter++;
                i++;

                return new Person
                {
                    AddressId = lastAddressId - i,
                    BirthDate = DateOnly.FromDateTime(f.Date.Past(50, DateTime.Now.AddYears(-18))),
                    FirstName = f.Name.FirstName(),
                    LastName = f.Name.LastName(),
                    Pesel = pesel
                };
            });

        var fakePeople = fakePeopleGenerator.Generate(count);

        await dbContext.BulkInsertAsync(fakePeople);
        return fakePeople;
    }

    private Address GenerateFakeAddress()
    {
        return new Address
        {
            Region = _faker.PickRandom(_wojewodztwa),
            City = _faker.Address.City(),
            Street = _faker.Address.StreetName(),
            HouseNumber = _faker.Random.Number(10_000)
        };
    }
}