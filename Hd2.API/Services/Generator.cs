using System.Collections.Concurrent;
using System.Diagnostics;
using Bogus;
using EFCore.BulkExtensions;
using Hd2.API.Data;
using Hd2.API.Models;
using Microsoft.EntityFrameworkCore;
using Person = Hd2.API.Models.Person;

namespace Hd2.API.Services;

public class Generator(HdDbContext dbContext, ILogger<Generator> logger)
{
    private const int NumberOfHospitals = 1;

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

        await GenerateProcedures(days);

        await GenerateComplications();
    }

    private async Task GenerateProcedures(int daysToGoBack)
    {
        List<Procedure> procedures = [];

        var hospitals = await dbContext
            .Hospitals
            .Include(h => h.Doctors)
            .ToListAsync();

        for (var i = 0; i < daysToGoBack; i++)
        {
            var today = DateTime.Now.AddDays(-i);
            foreach (var hospital in hospitals)
            {
                var doctors = hospital.Doctors.ToList();
                var doctorBatches = doctors.Chunk(2).ToList();

                var procedureCount = doctorBatches.Count;

                var donorsOrgansList = await GenerateDonorsAndOrgans(procedureCount, today);

                var patients = await GeneratePatients(procedureCount);

                var tempProcedures = doctorBatches
                    .Zip(donorsOrgansList, (doctor, donorOrgan) => (doctor, donorOrgan))
                    .Zip(patients, (doctorDonorOrgan, patient) => (doctorDonorOrgan, patient))
                    .Select(tempDataHolder => new
                    {
                        tempDataHolder.doctorDonorOrgan.doctor,
                        tempDataHolder.doctorDonorOrgan.donorOrgan.Item1,
                        tempDataHolder.doctorDonorOrgan.donorOrgan.Item2,
                        tempDataHolder.patient
                    })
                    .Select(a => GenerateProcedure(hospital.Id, today, a.patient.Pesel, a.doctor.ToList(), a.Item2.Id));

                procedures.AddRange(tempProcedures);
            }
        }

        await dbContext.BulkInsertAsync(procedures);
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

    private async Task<List<(Donor, Organ)>> GenerateDonorsAndOrgans(int count, DateTime today)
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

        var currentOrganCount = await dbContext.Organs.CountAsync();

        // generate organs
        var organs = donors.Select(donor => new Organ
        {
            HarvestDateTime = today.AddDays(-Random.Shared.Next(1, 14)),
            StorageType = _faker.PickRandom(_typyPrzechowywaniaNarzadow),
            OrganTypeId = _faker.PickRandom(organTypes).Id,
            DonorPesel = donor.Pesel
        }).ToList();

        await dbContext.BulkInsertAsync(organs);

        organs = await dbContext.Organs.Skip(currentOrganCount).ToListAsync();

        return donors
            .Zip(organs, (donor, organ) => (donor, organ))
            .ToList();
    }

    private Procedure GenerateProcedure(int hospitalId,
        DateTime today,
        string patientsPesel,
        ICollection<Doctor> doctors,
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
            Doctors = doctors,
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

        // reload addresses to get their ids
        randomAddresses = await dbContext.Addresses.ToListAsync();

        var fakePeopleGenerator = new Faker<Person>()
            .CustomInstantiator(f =>
            {
                var pesel = peselCounter.ToString("D11");

                peselCounter++;

                return new Person
                {
                    AddressId = randomAddresses[randomAddresses.Count - 1 - peselCounter].Id,
                    BirthDate = DateOnly.FromDateTime(f.Date.Past(18, DateTime.Now)),
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