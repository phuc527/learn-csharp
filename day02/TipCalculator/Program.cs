using System.Globalization;

// Day 2: Máy tính chia tiền tip.
// Tiền luôn dùng decimal (hậu tố m), không dùng double vì double có sai số.

decimal bill = ReadDecimal("Tổng hoá đơn: ", min: 0.01m, max: 1_000_000m);
int tipPercent = ReadInt("% tip (0-100): ", min: 0, max: 100);
int people = ReadInt("Số người: ", min: 1, max: 100);

decimal tip = RoundMoney(bill * tipPercent / 100m);
decimal total = bill + tip;

// Bonus: chia tiền không bị mất xu.
// Mỗi người trả phần làm tròn XUỐNG tới xu, số xu còn dư chia cho vài người, mỗi người thêm 0.01.
decimal baseShare = Math.Floor(total / people * 100m) / 100m;
int extraCents = (int)((total - baseShare * people) * 100m);
int normalPayers = people - extraCents;

Console.WriteLine();
Console.WriteLine($"{"Tiền tip:",-12}{tip,10:N2}");
Console.WriteLine($"{"Tổng cộng:",-12}{total,10:N2}");

if (extraCents == 0)
{
    Console.WriteLine($"{"Mỗi người:",-12}{baseShare,10:N2}");
}
else
{
    Console.WriteLine($"{normalPayers} người trả {baseShare:N2}, {extraCents} người trả {baseShare + 0.01m:N2}");
}

// Kiểm tra lại: tổng các phần phải đúng bằng total.
decimal check = baseShare * normalPayers + (baseShare + 0.01m) * extraCents;
if (check != total)
{
    Console.WriteLine($"LỖI: chia ra {check} nhưng cần {total}");
}

// ---------- Các hàm hỗ trợ (local functions) ----------

static decimal RoundMoney(decimal value) =>
    Math.Round(value, 2, MidpointRounding.AwayFromZero);

static decimal ReadDecimal(string prompt, decimal min, decimal max)
{
    while (true)
    {
        string input = Prompt(prompt);

        // InvariantCulture: dấu "." là dấu thập phân, không phụ thuộc cài đặt Windows.
        // Không dùng NumberStyles.Number vì nó cho phép dấu "," hàng nghìn → "12,5" thành 125 (sai gấp 10 lần!).
        const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        bool ok = decimal.TryParse(input, styles, CultureInfo.InvariantCulture, out decimal value);

        if (!ok)
        {
            Console.WriteLine($"  '{input}' không phải số hợp lệ (dùng dấu . cho số thập phân).");
        }
        else if (value < min || value > max)
        {
            Console.WriteLine($"  Phải nằm trong khoảng {min} đến {max:N0}.");
        }
        else
        {
            return RoundMoney(value);
        }
    }
}

static int ReadInt(string prompt, int min, int max)
{
    while (true)
    {
        string input = Prompt(prompt);

        if (!int.TryParse(input, out int value))
        {
            Console.WriteLine($"  '{input}' không phải số nguyên.");
        }
        else if (value < min || value > max)
        {
            Console.WriteLine($"  Phải nằm trong khoảng {min} đến {max}.");
        }
        else
        {
            return value;
        }
    }
}

static string Prompt(string message)
{
    Console.Write(message);
    // ReadLine trả về null khi hết input (vd. Ctrl+Z) → thoát thay vì lặp vô hạn.
    string? line = Console.ReadLine();
    if (line is null)
    {
        Console.WriteLine();
        Console.WriteLine("Không còn input, thoát.");
        Environment.Exit(1);
    }
    return line.Trim();
}
