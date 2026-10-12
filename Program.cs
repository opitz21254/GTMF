
using System;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.Title = "GTMF Opening Night - ticket threads";

        Console.WriteLine("Grand Teton Music Festival");
        Console.WriteLine("Opening Night: From the Stage to the Silver Screen");
        Console.WriteLine("Center for the Arts Lawn, Jackson, WY");
        Console.WriteLine("Donato Cabrera, conductor  |  reserved seats only");
        Console.WriteLine();
        Console.WriteLine("Several customers hit the box office at the same time.");
        Console.WriteLine("Each customer is its own Thread.");
        Console.WriteLine();

        Console.WriteLine("1) WITHOUT a lock  (what went wrong)");
        RunOneRush(useLock: false);

        Console.WriteLine();
        Console.Write("Press Enter to sell the same 12 seats again, this time WITH a lock...");
        Console.ReadLine();
        Console.WriteLine();

        Console.WriteLine("2) WITH a lock  (check + sell stay together)");
        RunOneRush(useLock: true);


        Console.Write("Press Enter to close...");
        Console.ReadLine();
    }

    static void RunOneRush(bool useLock)
    {
        BoxOffice booth = new BoxOffice(12, useLock);

        string[] names =
        {
            "Maya", "Luis", "the Chen family", "Priya",
            "Owen", "the Park family", "Sofia", "Nate",
            "Ava", "Jordan"
        };
        int[] want = { 2, 2, 4, 1, 3, 3, 2, 1, 2, 2 };


        Customer[] customers = new Customer[names.Length];
        Thread[] workers = new Thread[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            customers[i] = new Customer();
            customers[i].Name = names[i];
            customers[i].Wanted = want[i];
            customers[i].Booth = booth;

      
            workers[i] = new Thread(customers[i].Shop);
            workers[i].Name = names[i];
            workers[i].Start();
        }


        for (int i = 0; i < workers.Length; i++)
        {
            workers[i].Join();
        }

        int givenOut = 0;
        for (int i = 0; i < customers.Length; i++)
        {
            givenOut += customers[i].Got;
        }

        Console.WriteLine();
        booth.PrintTotals(givenOut);
    }
}

class BoxOffice
{
    int seatsLeft;
    int seatsAtStart;
    bool useLock;
    object gate = new object();

    public BoxOffice(int seats, bool useLock)
    {
        this.seatsLeft = seats;
        this.seatsAtStart = seats;
        this.useLock = useLock;
    }

    public int TryBuy(string name, int qty)
    {
        if (useLock)
        {
            lock (gate)
            {
                return TryBuyInner(name, qty);
            }
        }
        else
        {
            return TryBuyInner(name, qty);
        }
    }

    int TryBuyInner(string name, int qty)
    {

        int seen = seatsLeft;

        Thread.Sleep(80);

        if (seen >= qty)
        {
            seatsLeft = seen - qty;
            Console.WriteLine("  SOLD  " + qty + " to " + name +
                              "     (cashier thought " + seen + " were left)");
            return qty;
        }

        Console.WriteLine("  NO    " + name + " wanted " + qty +
                          " but cashier thought only " + seen + " were left");
        return 0;
    }

    public void PrintTotals(int givenOut)
    {
        Console.WriteLine("  ----");
        Console.WriteLine("  Seats in the hall:     " + seatsAtStart);
        Console.WriteLine("  Tickets handed out:    " + givenOut);
        Console.WriteLine("  seatsLeft variable:    " + seatsLeft);

        if (givenOut > seatsAtStart)
        {
            int extra = givenOut - seatsAtStart;
            Console.WriteLine("  OVERSOLD by " + extra +
                              "  (two cashiers sold the same seats)");
        }
        else if (seatsLeft < 0)
        {
            Console.WriteLine("  seatsLeft went negative — same bug as overselling.");
        }
        else
        {
            Console.WriteLine("  Did not oversell. lock kept the count honest.");
        }
    }
}

class Customer
{
    public string Name;
    public int Wanted;
    public int Got;
    public BoxOffice Booth;

    public void Shop()
    {
        Thread.Sleep(20);
        Got = Booth.TryBuy(Name, Wanted);
    }
}
