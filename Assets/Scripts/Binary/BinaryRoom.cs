public class BinaryRoom:Room
{
    public BinaryRoom[] dawnTreeRooms;
    public bool leftBorder;
    public bool rightBorder;
    public bool bottomBorder;
    public bool topBorder;
    public bool IsOuter()
    {
        return leftBorder || rightBorder || bottomBorder || topBorder;
    }
    public BinaryRoom(int left, int right, int bottom, int top)
    {
        this.Left = left;
        this.Right = right;
        this.Bottom = bottom;
        this.Top = top;
    }

    public void ChangeSize(int leftOffset, int rightOffset, int bottomOffset, int topOffset)
    {
        Left += leftOffset;
        Right += rightOffset;
        Bottom += bottomOffset;
        Top += topOffset;
    }
}