for (var i = 0; i < 10; i++)
    Console.WriteLine($"> {i}");

Log.Progress = 0;

var l1 = Log.AddStatusLine();
var l2 = Log.AddStatusLine();
var l3 = Log.AddStatusLine("ahoy!");
var l4 = Log.AddStatusLine();
var l5 = Log.AddStatusLine();

//for (var i = 20; i < 21; i++)
//    Console.WriteLine($"> {i}");

Thread.Sleep(1000);
l4.Text = "w00t";
l5.Text = "Last";

Log.Progress = 0.2f;

Thread.Sleep(1000);
l1.Text = "w00t";

Log.Progress = 0.5f;

Thread.Sleep(1000);
l4.Text = "w00t000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000008888----";
l1.Prefix = "Top item";

Log.Progress = 0.7f;

Thread.Sleep(1000);
l3.Text = "line 1\nline 2\nline 3\nline 4\nline 5";

Log.Progress = 0.9f;

for (var i=0; ; i++)
{
    l2.Text = "---------------------------------------------------" + DateTime.Now.ToString();
    l5.Text = "Last - " + DateTime.Now.ToString();
    Thread.Sleep(250);

    Log.Progress = 1f;

    if((i%30)==10)
        l3.Remove();
    if ((i % 30) == 25)
        l3=Log.AddStatusLine(DateTime.Now.ToString());

    l1.Progress = 0.5f+ 0.6f*MathF.Sin(i/5f);
    l3.Progress = 0.7f + 0.8f * MathF.Sin(i / 4f);

    Console.WriteLine(i);
}
