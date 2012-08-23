

extern "C"
{



struct BBox
{
	float x1, y1, x2, y2;
	int isValid()
	{
		return (x1 < 1) && (x2 < 1) && (y1 < 1) && (y2 < 1) && (x1 > 0) && (x2 > 0) && (y1 > 0) && (y2 > 0);
	}
};


__declspec(dllexport)  float __cdecl nativef(BBox * boxes, int size)
{
	int sum = 0;
	for (int i = 0; i < size; i++)
	{
		sum+= boxes[i].isValid();
	}
	return (float)sum/(float)size * 100;
}

};