import os
import sys
from PIL import Image, ImageOps

def kompresuj(input, output, max_wymiar=2000, max_kb=500):

    img = Image.open(input)
    
    #obróć do pionu
    img = ImageOps.exif_transpose(img)

    #usuń przezroczystość
    if img.mode in ("RGBA", "P"):
        img = img.convert("RGB")

    docelowy_rozmiar = max_kb * 1024
    
    #zmniejsz do docelowego wymiaru
    if max(img.size) > max_wymiar:
        img.thumbnail((max_wymiar, max_wymiar))


    #zmniejszaj jakosc
    #jesli nie uda sie osiagnac docelowego to po prostu zapisze to na jakosci 25%
    jakosc = 100
    min_jakosc = 25
    krok = 5
    aktualny_rozmiar = 0

    while jakosc >= min_jakosc:
        img.save(output, "JPEG", quality=jakosc, optimize=True)
        aktualny_rozmiar = os.path.getsize(output)
        
        if aktualny_rozmiar <=docelowy_rozmiar:
            break
            
        jakosc -= krok

    
    #img.show()

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Brak argumentów wejściowych", file=sys.stderr)
        sys.exit(1)
        
    wejscie = sys.argv[1]
    wyjscie = sys.argv[2]
    kompresuj(wejscie, wyjscie)