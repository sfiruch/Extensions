for (var i = 0; i < 20; i++)
    Console.WriteLine($"> {i}");

Log.Progress = 0;

var ml = new Log.MultiLine();
var l1 = ml.AddLine();
var l2 = ml.AddLine();
var l3 = ml.AddLine("ahoy!");
var l4 = ml.AddLine();
var l5 = ml.AddLine();

Thread.Sleep(1000);
l4.Text = "w00t";
l5.Text = "Last";

Log.Progress = 0.2f;

Thread.Sleep(1000);
l1.Text = "w00t";

Log.Progress = 0.5f;

Thread.Sleep(1000);
l4.Text = "w00t000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000008888----";

Log.Progress = 0.7f;

Thread.Sleep(1000);
l3.Text = "line 1\nline 2";

Log.Progress = 0.9f;

for (; ; )
{
    l2.Text = "---------------------------------------------------" + DateTime.Now.ToString();
    l5.Text = "Last - " + DateTime.Now.ToString();
    Thread.Sleep(200);

    Log.Progress = 1f;
}
