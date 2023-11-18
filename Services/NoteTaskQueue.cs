using System.Threading.Channels;

public class NoteTaskQueue
{
    private readonly Channel<NoteTask> _queue;
    private readonly int capacity;
    private int count;

    public NoteTaskQueue(int capacity)
    {
        this.capacity = capacity;
        count = 0;

        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<NoteTask>(options);
    }

    public async Task<bool> EnqueueAsync(NoteTask workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        if (count >= capacity)
        {
            return false;
        }

        await _queue.Writer.WriteAsync(workItem);
        return true;
    }

    public async Task<NoteTask> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}