/*****************************************************************************************

This is LLD of a parking lot system which have following features:-
1. Multiple Entry/Exit gates.
2. Can use multiple techniques to assign a spot to park a vehicle.
3. Can use multiple ways to charging fee.

*******************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

Console.WriteLine("Start executing main.....");
var floor1 = new ParkingFloor(1, new List<ParkingSpot>
        {
            new CompactSpot("1A"),
            new LargeSpot("1B")
        });

var parkingLot = new ParkingLot(new List<ParkingFloor> { floor1 });
var entryGates = new List<EntryGate>
{
    new EntryGate(new DefaultParkingSpotAssigner()),
    new EntryGate(new DefaultParkingSpotAssigner())
};

var exitGates = new List<ExitGate>
{
    new ExitGate(new HourlyRatePricingStrategy(20)),
    new ExitGate(new HourlyRatePricingStrategy(20))
};
var manager = new ParkingLotManager(parkingLot, entryGates, exitGates);

//manager.ShowAvailableSpots();

var vehicle = new Car("DL01AB1234");
var ticket = manager.VehicleEntry(vehicle, 0);
Console.WriteLine($"Ticket ID: {ticket.TicketId}");

Thread.Sleep(2000); // simulate time

var receipt = manager.VehicleExit(ticket.TicketId, 1);
Console.WriteLine($"Receipt ID: {receipt.ReceiptId}, Fee: Rs {receipt.Fee}");

/*
* Vehicle Classes 
*/
public enum VehicleType
{
    Bike, Car, Bus
}

public abstract class Vehicle
{
    public string LicenceNumber { get; set; }
    public VehicleType Type { get; protected set; }

    protected Vehicle(string licenseNumber, VehicleType vehicleType)
    {
        LicenceNumber = licenseNumber;
        Type = vehicleType;
    }
}

public class Car : Vehicle
{
    public Car(string licenseNumber)
        : base(licenseNumber, VehicleType.Car)
    {
    }
}

public class Bus : Vehicle
{
    public Bus(string licenseNumber)
        : base(licenseNumber, VehicleType.Bus)
    {
    }
}

public class Bike : Vehicle
{
    public Bike(string licenseNumber)
        : base(licenseNumber, VehicleType.Bike)
    {
    }
}

/*
 * Parking Spot classes
*/

public enum SpotType
{ 
    Compact, Large, Handicapped
}

public abstract class ParkingSpot
{
    public string SpotId { get; set; }
    public bool IsAvailable { get; protected set; }
    public Vehicle? ParkedVehicle { get; protected set; }
    public SpotType SpotType { get; protected set; }

    protected ParkingSpot(string spotId)
    {
        SpotId = spotId;
    }

    public virtual bool CanFitVehicle(Vehicle vehicle)
    {
        return !IsAvailable;
    }

    public bool AssignVehicle(Vehicle vehicle)
    {
        if (CanFitVehicle(vehicle))
        {
            ParkedVehicle = vehicle;
            IsAvailable = true;
            return true;
        }
        return false;
    }

    public void RemoveVehicle()
    {
        if (!IsAvailable)
        {
            ParkedVehicle = null;
            IsAvailable = false;
        }
    }
}

public class CompactSpot : ParkingSpot
{
    public CompactSpot(string spotId)
        : base(spotId)
    {
        SpotType = SpotType.Compact;
    }

    public override bool CanFitVehicle(Vehicle vehicle)
    {
        return base.CanFitVehicle(vehicle) && vehicle.Type == VehicleType.Bike || vehicle.Type == VehicleType.Car;
    }
}

public class LargeSpot : ParkingSpot
{
    public LargeSpot(string spotId)
    : base(spotId)
    {
        SpotType = SpotType.Large;
    }

    public override bool CanFitVehicle(Vehicle vehicle)
    {
        return base.CanFitVehicle(vehicle);
    }
}

public class HandicappedSpot : ParkingSpot
{
    public HandicappedSpot(string spotId)
    : base(spotId)
    {
        SpotType = SpotType.Handicapped;
    }

    public override bool CanFitVehicle(Vehicle vehicle)
    {
        return base.CanFitVehicle(vehicle);
    }
}

public class Ticket
{
    public string TicketId;
    public DateTime EntryTime;
    public Vehicle vehicle;
    public ParkingSpot Spot;

    public Ticket(string ticketId, Vehicle vehicle, ParkingSpot spot)
    {
        TicketId = ticketId;
        EntryTime = DateTime.Now;
        this.vehicle = vehicle;
        Spot = spot;
    }
}

public class Receipt
{
    public string ReceiptId;
    public Ticket Ticket;
    public DateTime ExitTime;
    public double Fee;

    public Receipt(string receiptId, Ticket ticket, DateTime exitTime, double fee)
    {
        ReceiptId = receiptId;
        Ticket = ticket;
        ExitTime = exitTime;
        Fee = fee;
    }
}

/*
 * I am using pricing strategy so that we can change how we want to calculate pricing.
 */
public interface IPricingStrategy
{
    public double CalculateFee(Ticket ticket, DateTime exitTime);
}

public class HourlyRatePricingStrategy : IPricingStrategy
{
    private readonly double _ratePerHour;

    public HourlyRatePricingStrategy(double ratePerHour)
    {
        _ratePerHour = ratePerHour;
    }

    public double CalculateFee(Ticket ticket, DateTime exitTime)
    {
        var hours = (exitTime - ticket.EntryTime).TotalHours;
        return Math.Ceiling(hours) * _ratePerHour;
    }
}

public class VehicleTypeBasedPricingStrategy : IPricingStrategy
{
    private readonly Dictionary<VehicleType, double> _hourlyRates;

    public VehicleTypeBasedPricingStrategy()
    {
        _hourlyRates = new Dictionary<VehicleType, double>
        {
            { VehicleType.Car, 20 },
            { VehicleType.Bike, 10 },
            { VehicleType.Bus, 50 }, // Add more if needed
        };
    }

    public double CalculateFee(Ticket ticket, DateTime exitTime)
    {
        TimeSpan duration = exitTime - ticket.EntryTime;
        int hours = (int)Math.Ceiling(duration.TotalHours);

        if (_hourlyRates.TryGetValue(ticket.vehicle.Type, out double rate))
        {
            return hours * rate;
        }

        throw new Exception("Unsupported vehicle type for pricing.");
    }
}

/*
 * ParkingFloor Class
 */
public class ParkingFloor
{
    public int FloorNumber;
    public List<ParkingSpot> Spots;

    public ParkingFloor(int floorNumber, List<ParkingSpot> spots)
    {
        FloorNumber = floorNumber;
        Spots = spots;
    }
}

/*
 * ParkingLot Class
 */
public class ParkingLot
{
    public List<ParkingFloor> Floors;

    public ParkingLot(List<ParkingFloor> floors)
    {
        Floors = floors;
    }
}

/*
 * I am using strategy pattern here so that we use how we want to assign the spot.
 */
public interface IParkingSpotAssigner
{
    ParkingSpot? AssignSpot(Vehicle vehicle, ParkingLot parkingLot);
}

public class DefaultParkingSpotAssigner : IParkingSpotAssigner
{
    public ParkingSpot? AssignSpot(Vehicle vehicle, ParkingLot parkingLot)
    {
        foreach(var floor in parkingLot.Floors)
        {
            foreach(var spot in floor.Spots)
            {
                if(spot != null && !spot.IsAvailable)
                {
                    return spot;
                }
            }
        }

        return null;
    }
}

/*
 * EntryGate class.
 */
public class EntryGate
{
    private readonly IParkingSpotAssigner _spotAssiner;

    public EntryGate(IParkingSpotAssigner spotAssiner)
    {
        _spotAssiner = spotAssiner;
    }

    public Ticket ProcessEntry(Vehicle vehicle, ParkingLot parkingLot)
    {
        var spot = _spotAssiner.AssignSpot(vehicle, parkingLot);
        spot.AssignVehicle(vehicle);

        return new Ticket(Guid.NewGuid().ToString(), vehicle, spot);
    }
}

/*
 * ExitGate class.
 */
public class ExitGate
{
    private readonly IPricingStrategy _pricingStrategy;

    public ExitGate(IPricingStrategy pricingStrategy)
    {
        _pricingStrategy = pricingStrategy;
    }
    public Receipt ProcessExit(Ticket ticket)
    {
        var exitTime = DateTime.Now;
        var fee = _pricingStrategy.CalculateFee(ticket, exitTime);

        ticket.Spot.RemoveVehicle();

        return new Receipt(Guid.NewGuid().ToString(), ticket, exitTime, fee);
    }
}

/*
 * Main class that manages the whole system.
 */
public class ParkingLotManager
{
    private readonly ParkingLot _parkingLot;

    private readonly List<EntryGate> _entryGates;

    private readonly List<ExitGate> _exitGates;

    private readonly Dictionary<string, Ticket> _activeTickets = new Dictionary<string, Ticket>();

    public ParkingLotManager(ParkingLot parkingLot, List<EntryGate> entryGates, List<ExitGate> exitGates)
    {
        _parkingLot = parkingLot;
        _entryGates = entryGates;
        _exitGates = exitGates;
    }

    public Ticket VehicleEntry(Vehicle vehicle, int gateIndex)
    {
        var entryGate = _entryGates[gateIndex];
        var ticket = entryGate.ProcessEntry(vehicle, _parkingLot);
        _activeTickets[ticket.TicketId] = ticket;
        return ticket;
    }

    public Receipt? VehicleExit(string ticketId, int gateIndex)
    {
        if(!_activeTickets.TryGetValue(ticketId, out var ticket))
        {
            Console.WriteLine("Invalid ticket");
            return null;
        }

        var exitGate = _exitGates[gateIndex];
        var receipt = exitGate.ProcessExit(ticket);
        _activeTickets.Remove(ticketId);
        return receipt;
    }
}
