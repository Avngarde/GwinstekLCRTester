# GwinstekLCRTester

GwinstekLCRTester jest to program napisany w C# przy użyciu WPF, służący do automatyzacji testów urządzeń mierniczych z serii Gwinstek LCR 6000

## Nawigacja
* [Funkcjonalności aplikacji](#Funkcjonalności-aplikacji)
* [Opis GUI](#Opis-GUI)
* [Opis struktury obiektów](#Opis-struktury-obiektów)
* [Kompatybilność sprzętowa](##Kompatybilność-sprzętowa)
* [Uwagi i wyjaśnienia](##Uwagi)
* [Użyte technologie](##Użyte-technologie)
* [Licencja](##Licencja)
* [Autorzy](##Autorzy)



## Funkcjonalności aplikacji

Aplikacja umożliwia zautomatyzowane mierzenie kondensatorów na podstawie podanych przez użytkownika wartości. Takie pomiary są zapisywane do plików csv w celu prostego porównywania danych w programach skoroszytowych. Mamy dwa sposoby mierzenia:

* **Test wielu kondensatorów (domyślny)** : Dla tego trybu podajemy częstotliwości mierzenia oraz (liczbę cykli)[##Uwagi-i-wyjaśnienia]. Po wciśnięciu przycisku "Wykonaj testy" program pokazuje na jakich wpisanych parametrach wykona pomiary oraz prosi o podłączenie kondensatora do miernika. Po kliknięciu "OK" wykonywane są testy dla częstotliwości powtarzane przez ilość cykli. Po zakończeniu pomiarów wyświetla się następne okinko z prośbą o podłączenie następnego urządzenia. Jeśli chcemy przerwać testy klikamy "Cancel"

* **Test seryjny** : Jest to tryb pomiaru jednego kondensatora. Mamy tutaj do podania (parametr AVG)[##Uwagi-i-wyjaśnienia], który odpowiada za uśrednianie każdego pomiaru, oraz liczbę cykli. Program po wykonaniu zadanych obliczeń poinformuje nas o zakończeniu testów



## Opis GUI

Do rozpoczęcia pracy z programem niezbędne jest podłączenie miernika Gwinstek kablem RS-232 z przejściówką USB do komputera. Aby upewnić się, że połączenie zostało ustawione można przejść do Menedżera Urządzeń, a następnie sprawdzić zakładkę Porty(COM i LPT1). Program po włączeniu automatycznie sprawdza aktywne porty COM i jeśli je znajduje pokazuje swoje GUI. GUI dzieli się na 3 panele:

* Panel lewy : Służy do ustawienia parametrów połączenia RS takich jak szybkość transmisjii, bitów stopu, bitów parzystości itp. Wszystkie parametry muszą być w 100% zgodne z specyfikacją miernika 

* Panel środkowy : Tutaj ustalamy częstotliwości pojedyńczych testów, tryb pomiaru, [mnożnik jednostek pomiaru](##Uwagi) oraz [opcjonalny parametr D](##Uwagi)

* Panel prawy : Odpowiada za ustalanie [cykli pomiarów](##Uwagi), [testów seryjnych](##Uwagi) oraz pozwala na ustalenie swojej własnej ścieżki wyjściowej dla plików csv


## Opis struktury obiektów

lorem ipsum sid dolor
