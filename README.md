# GwinstekLCRTester

GwinstekLCRTester jest to program napisany w C# przy użyciu WPF, służący do automatyzacji testów urządzeń mierniczych z serii Gwinstek LCR 6000

## Nawigacja
* [Funkcjonalności aplikacji](##Funkcjonalności aplikacji)
* [Opis struktury obiektów](##Opis struktury obiektów)
* [Kompatybilność sprzętowa](##Kompatybilność sprzętowa)
* [Uwagi](##Uwagi)
* [Użyte technologie](##Użyte technologie)
* [Licencja](##Licencja)
* [Autorzy](##Autorzy)



##Funkcjonalności aplikacji

Do rozpoczęcia pracy z programem niezbędne jest podłączenie miernika Gwinstek kablem RS-232 z przejściówką USB do komputera. Aby upewnić się, że połączenie zostało ustawione można przejść do Menedżera Urządzeń, a następnie sprawdzić zakładkę Porty(COM i LPT1). Program po włączeniu automatycznie sprawdza aktywne porty COM i jeśli je znajduje pokazuje swoje GUI. GUI dzieli się na 3 panele:

* Panel lewy : Służy do ustawienia parametrów połączenia RS takich jak szybkość transmisjii, bitów stopu, bitów parzystości itp. Wszystkie parametry muszą być w 100% zgodne z specyfikacją miernika 
* Panel środkowy : Tutaj ustalamy częstotliwości pojedyńczych testów, tryb pomiaru, [mnożnik jednostek pomiaru](##Uwagi) oraz [opcjonalny parametr D](##Uwagi)
* Panel prawy : Odpowiada za ustalanie [cykli pomiarów](##Uwagi), [testów seryjnych](##Uwagi) oraz pozwala na ustalenie swojej własnej ścieżki wyjściowej dla plików csv


