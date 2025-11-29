# Dokumentacja Backendu CarRental

## Spis Treści
1. [Endpointy API](#endpointy-api)
2. [Baza Danych](#baza-danych)
3. [Relacje](#relacje)
4. [Procedury Składowane](#procedury-składowane)
5. [Funkcje Użytkownika](#funkcje-użytkownika)
6. [Wyzwalacze (Triggers)](#wyzwalacze-triggers)
7. [Autoryzacja i Uwierzytelnianie](#autoryzacja-i-uwierzytelnianie)
8. [Obsługa Błędów](#obsługa-błędów)
9. [Logowanie](#logowanie)

## Endpointy API

### AuthController (Uwierzytelnianie)
Kontroler odpowiedzialny za rejestrację, logowanie i zarządzanie tokenami użytkowników.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Auth/register` | Rejestracja nowego użytkownika. | Brak |
| `POST` | `/api/Auth/login` | Logowanie użytkownika. Zwraca token JWT. | Brak |
| `POST` | `/api/Auth/refresh-token` | Odświeżenie tokena dostępu (Refresh Token). | Brak |
| `GET` | `/api/Auth/me` | Pobranie danych aktualnie zalogowanego użytkownika. | Zalogowany |
| `GET` | `/api/Auth/admin` | Testowy endpoint dostępny tylko dla administratora. | Admin |

### CarController (Samochody)
Kontroler służący do przeglądania i zarządzania dostępnymi samochodami.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/Car` | Pobranie listy wszystkich samochodów. | Brak |
| `GET` | `/api/Car/{id}` | Pobranie szczegółów konkretnego samochodu po ID. | Brak |
| `POST` | `/api/Car` | Utworzenie nowego samochodu w systemie. | Admin |
| `PUT` | `/api/Car/{id}` | Aktualizacja danych samochodu. | Admin |
| `DELETE` | `/api/Car/{id}` | Usunięcie samochodu z systemu. | Admin |

### ReservationController (Rezerwacje)
Kontroler do zarządzania rezerwacjami samochodów.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Reservation` | Utworzenie nowej rezerwacji. Weryfikuje dostępność samochodu w podanym terminie. | Zalogowany |
| `GET` | `/api/Reservation` | Pobranie wszystkich rezerwacji aktualnie zalogowanego użytkownika. | Zalogowany |
| `DELETE` | `/api/Reservation/{id}` | Anulowanie własnej rezerwacji. | Zalogowany |
| `GET` | `/api/Reservation/admin/all` | Pobranie wszystkich rezerwacji w systemie. | Admin |
| `DELETE` | `/api/Reservation/admin/{id}` | Anulowanie dowolnej rezerwacji przez administratora. | Admin |

### ReviewController (Recenzje)
Kontroler do zarządzania recenzjami samochodów.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/Review/car/{carId}` | Pobranie wszystkich recenzji dla danego samochodu. | Brak |
| `POST` | `/api/Review` | Utworzenie nowej recenzji. Użytkownik może dodać recenzję tylko dla samochodu, który wcześniej zarezerwował. | Zalogowany |
| `PUT` | `/api/Review/{id}` | Aktualizacja własnej recenzji. | Zalogowany |
| `DELETE` | `/api/Review/{id}` | Usunięcie własnej recenzji. | Zalogowany |
| `GET` | `/api/Review/all` | Pobranie wszystkich recenzji w systemie. | Admin |

### ReportsController (Raporty)
Kontroler do generowania raportów i analiz. Dostępny tylko dla administratorów.

| Metoda | Endpoint | Opis | Wymagane Uprawnienia |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/Reports/monthly-revenue` | Generowanie raportu miesięcznych przychodów. Parametry opcjonalne: `year`, `month`. | Admin |
| `GET` | `/api/Reports/user-history/{userId}` | Pobranie historii rezerwacji użytkownika z statusami (Completed, Active, Upcoming). | Admin |
| `GET` | `/api/Reports/discount/{userId}` | Obliczenie zniżki dla użytkownika na podstawie liczby rezerwacji (5% dla 5+, 10% dla 10+). | Admin |
| `GET` | `/api/Reports/reservation-logs` | Pobranie wszystkich logów działań związanych z rezerwacjami dla wszystkich użytkowników w systemie. Logi są sortowane od najnowszych do najstarszych. Nie wymaga parametrów - zwraca kompletną historię wszystkich akcji na rezerwacjach. | Admin |

---

## Baza Danych

### Tabela: `Users` (Użytkownicy)
Przechowuje informacje o użytkownikach systemu.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `Guid` | Unikalny identyfikator użytkownika (Klucz Główny). |
| `Email` | `string` | Adres email użytkownika. |
| `PasswordHash` | `string` | Zahaszowane hasło. |
| `Role` | `string` | Rola użytkownika (np. "Admin", "User"). |
| `RefreshToken` | `string?` | Token odświeżający. |
| `RefreshTokenExpiryTime` | `DateTime?` | Data wygaśnięcia tokena odświeżającego. |

### Tabela: `Cars` (Samochody)
Przechowuje informacje o flocie samochodowej.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `int` | Unikalny identyfikator samochodu (Klucz Główny). |
| `Brand` | `string` | Marka samochodu. |
| `Model` | `string` | Model samochodu. |
| `Year` | `int` | Rok produkcji. |
| `PricePerDay` | `decimal` | Cena za dzień wynajmu. |
| `Description` | `string?` | Opis samochodu. |
| `ImageUrl` | `string?` | URL do zdjęcia samochodu. |

### Tabela: `Reservations` (Rezerwacje)
Przechowuje informacje o rezerwacjach dokonanych przez użytkowników.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `int` | Unikalny identyfikator rezerwacji (Klucz Główny). |
| `CarId` | `int` | ID rezerwowanego samochodu (Klucz Obcy -> Cars). |
| `UserId` | `Guid` | ID użytkownika dokonującego rezerwacji (Klucz Obcy -> Users). |
| `StartDateTime` | `DateTime` | Data i czas rozpoczęcia rezerwacji. |
| `EndDateTime` | `DateTime` | Data i czas zakończenia rezerwacji. |
| `TotalPrice` | `decimal` | Całkowity koszt rezerwacji (obliczany automatycznie). |
| `CreatedAt` | `DateTime` | Data utworzenia rezerwacji. |

**Indeksy:**
- `IX_Reservations_CarId` - Indeks na kolumnie `CarId` dla szybkiego wyszukiwania rezerwacji po samochodzie.
- `IX_Reservations_UserId` - Indeks na kolumnie `UserId` dla szybkiego wyszukiwania rezerwacji użytkownika.

### Tabela: `Reviews` (Recenzje)
Przechowuje recenzje samochodów napisane przez użytkowników.

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `int` | Unikalny identyfikator recenzji (Klucz Główny). |
| `Rating` | `int` | Ocena w skali 1-5 (walidacja przez `[Range(1, 5)]`). |
| `Comment` | `string?` | Tekst recenzji (opcjonalny). |
| `CarId` | `int` | ID recenzowanego samochodu (Klucz Obcy -> Cars). |
| `UserId` | `Guid` | ID użytkownika piszącego recenzję (Klucz Obcy -> Users). |
| `CreatedAt` | `DateTime` | Data utworzenia recenzji. |

**Indeksy:**
- `IX_Reviews_CarId` - Indeks na kolumnie `CarId` dla szybkiego wyszukiwania recenzji po samochodzie.
- `IX_Reviews_UserId` - Indeks na kolumnie `UserId` dla szybkiego wyszukiwania recenzji użytkownika.

### Tabela: `ReservationLogs` (Logi Rezerwacji)
Przechowuje logi działań związanych z rezerwacjami (automatycznie wypełniana przez trigger).

| Kolumna | Typ | Opis |
| :--- | :--- | :--- |
| `Id` | `int` | Unikalny identyfikator logu (Klucz Główny). |
| `ReservationId` | `int` | ID rezerwacji, której dotyczy log. |
| `UserId` | `Guid` | ID użytkownika wykonującego akcję. |
| `Action` | `string` | Typ akcji (np. "Deleted", "Created"). |
| `LogDate` | `DateTime` | Data i czas wykonania akcji. |

---

## Relacje

1.  **Users ↔ Reservations**: Relacja jeden-do-wielu (One-to-Many).
    -   Jeden użytkownik (`User`) może mieć wiele rezerwacji (`Reservations`).
    -   Każda rezerwacja jest przypisana do jednego użytkownika.
    -   Klucz obcy: `Reservations.UserId` → `Users.Id` (Cascade Delete).

2.  **Cars ↔ Reservations**: Relacja jeden-do-wielu (One-to-Many).
    -   Jeden samochód (`Car`) może być przedmiotem wielu rezerwacji (w różnym czasie).
    -   Każda rezerwacja dotyczy jednego konkretnego samochodu.
    -   Klucz obcy: `Reservations.CarId` → `Cars.Id` (Cascade Delete).

3.  **Users ↔ Reviews**: Relacja jeden-do-wielu (One-to-Many).
    -   Jeden użytkownik (`User`) może napisać wiele recenzji (`Reviews`).
    -   Każda recenzja jest przypisana do jednego użytkownika.
    -   Klucz obcy: `Reviews.UserId` → `Users.Id` (Cascade Delete).

4.  **Cars ↔ Reviews**: Relacja jeden-do-wielu (One-to-Many).
    -   Jeden samochód (`Car`) może mieć wiele recenzji (`Reviews`).
    -   Każda recenzja dotyczy jednego konkretnego samochodu.
    -   Klucz obcy: `Reviews.CarId` → `Cars.Id` (Cascade Delete).

---

## Procedury Składowane

### GetMonthlyRevenue
Generuje raport przychodów dla określonego miesiąca.

**Parametry:**
- `@Year INT = NULL` - Rok (domyślnie: bieżący rok)
- `@Month INT = NULL` - Miesiąc (domyślnie: bieżący miesiąc)

**Zwraca:**
- `Year` - Rok
- `Month` - Miesiąc
- `TotalReservations` - Całkowita liczba rezerwacji
- `TotalRevenue` - Całkowity przychód
- `AverageReservationValue` - Średnia wartość rezerwacji

**Użycie:**
```sql
EXEC GetMonthlyRevenue @Year = 2024, @Month = 12
```

**Endpoint API:** `GET /api/Reports/monthly-revenue?year=2024&month=12`

### GetUserReservationHistory
Pobiera historię rezerwacji użytkownika z automatycznym określeniem statusu.

**Parametry:**
- `@UserId UNIQUEIDENTIFIER` - ID użytkownika

**Zwraca:**
- `Id` - ID rezerwacji
- `CarId` - ID samochodu
- `Brand` - Marka samochodu
- `Model` - Model samochodu
- `StartDateTime` - Data rozpoczęcia
- `EndDateTime` - Data zakończenia
- `TotalPrice` - Całkowita cena
- `CreatedAt` - Data utworzenia
- `Status` - Status rezerwacji ('Completed', 'Active', 'Upcoming')

**Użycie:**
```sql
EXEC GetUserReservationHistory @UserId = '...'
```

**Endpoint API:** `GET /api/Reports/user-history/{userId}`

---

## Funkcje Użytkownika

### CalculateDiscount
Oblicza procent zniżki dla użytkownika na podstawie liczby jego rezerwacji.

**Parametry:**
- `@UserId UNIQUEIDENTIFIER` - ID użytkownika

**Zwraca:**
- `DECIMAL(5,2)` - Procent zniżki (0.00, 5.00 lub 10.00)

**Logika:**
- 0% - mniej niż 5 rezerwacji
- 5% - 5 lub więcej rezerwacji
- 10% - 10 lub więcej rezerwacji

**Użycie:**
```sql
SELECT dbo.CalculateDiscount(@UserId) AS Discount
```

**Endpoint API:** `GET /api/Reports/discount/{userId}`

---

## Wyzwalacze (Triggers)

### LogReservationDelete
Automatycznie loguje usunięcie rezerwacji do tabeli `ReservationLogs`.

**Typ:** `AFTER DELETE`  
**Tabela:** `Reservations`

**Działanie:**
- Po usunięciu rezerwacji automatycznie tworzy wpis w tabeli `ReservationLogs`
- Zapisuje: `ReservationId`, `UserId`, `Action = 'Deleted'`, `LogDate = GETDATE()`

**Zapewnia:**
- Pełną historię działań na rezerwacjach
- Możliwość audytu zmian w systemie
- Logowanie działań użytkowników (wymaganie funkcjonalne projektu)

**Uwaga:** Obecnie trigger loguje tylko akcję "Deleted". W przyszłości można rozszerzyć system o logowanie innych akcji (np. "Created", "Cancelled") poprzez dodatkowe triggery lub bezpośrednie zapisywanie w kodzie aplikacji.

---

## Autoryzacja i Uwierzytelnianie

### Mechanizm JWT (JSON Web Tokens)
System używa JWT do autoryzacji i uwierzytelniania użytkowników.

**Konfiguracja:**
- Schemat: `JwtBearerDefaults.AuthenticationScheme`
- Walidacja: Issuer, Audience, Lifetime, SigningKey
- Parametry konfiguracyjne w zmiennych środowiskowych:
  - `JWT_ISSUER`
  - `JWT_AUDIENCE`
  - `JWT_KEY`

**Tokeny:**
- **Access Token** - krótkotrwały token dostępu (zawiera: Id, Email, Role)
- **Refresh Token** - długotrwały token do odświeżania (przechowywany w bazie danych)

**Role:**
- `"User"` - Zwykły użytkownik
- `"Admin"` - Administrator

**Atrybuty autoryzacji:**
- `[Authorize]` - Wymaga zalogowania
- `[Authorize(Roles = "Admin")]` - Wymaga roli administratora

**Endpoints autoryzacji:**
- `POST /api/Auth/register` - Rejestracja (bez autoryzacji)
- `POST /api/Auth/login` - Logowanie (bez autoryzacji)
- `POST /api/Auth/refresh-token` - Odświeżanie tokenu (bez autoryzacji)
- `GET /api/Auth/me` - Pobranie danych użytkownika (wymaga autoryzacji)

**Automatyczne tworzenie konta admina:**
- Przy starcie aplikacji automatycznie tworzone jest konto administratora (jeśli nie istnieje)
- Metoda: `AuthService.EnsureAdminAccountExistsAsync()`

---

## Obsługa Błędów

### ExceptionHandlingMiddleware
Globalny middleware do obsługi wyjątków w całej aplikacji.

**Lokalizacja:** `CarRentalBackend/Middleware/ExceptionHandlingMiddleware.cs`

**Obsługiwane wyjątki:**
- `InvalidOperationException` → `400 Bad Request` (z komunikatem błędu)
- `UnauthorizedAccessException` → `401 Unauthorized`
- `KeyNotFoundException` → `404 Not Found` (z komunikatem błędu)
- Inne wyjątki → `500 Internal Server Error` (ogólny komunikat)

**Logowanie:**
- Wszystkie wyjątki są logowane przez Serilog
- Zawiera: metodę HTTP, ścieżkę, komunikat błędu, stack trace

**Użycie:**
- Middleware jest rejestrowany w `Program.cs` przed `UseAuthorization()`
- Automatycznie przechwytuje wszystkie nieobsłużone wyjątki

**Dodatkowa obsługa w kontrolerach:**
- Try-catch bloki w kontrolerach dla specyficznych przypadków
- Walidacja przez `ModelState.IsValid`
- Zwracanie odpowiednich kodów HTTP (400, 401, 404, 500)

---

## Logowanie

### Serilog
System używa Serilog do logowania zdarzeń i błędów.

**Konfiguracja:**
- Lokalizacja: `Program.cs:17-19`
- Sink: Pliki tekstowe
- Ścieżka: `logs/carrental-YYYYMMDD.txt`
- Format: Rolling interval dzienny

**Logowane zdarzenia:**
- Błędy aplikacji (ExceptionHandlingMiddleware)
- Błędy SQL (ReportsController)
- Informacje o operacjach (migracje, procedury składowane)
- Operacje na rezerwacjach (przez trigger)

**Poziomy logowania:**
- `Log.Information()` - Informacje o operacjach
- `Log.Warning()` - Ostrzeżenia
- `Log.Error()` - Błędy z pełnym stack trace

**Dodatkowe logowanie:**
- `ReservationLogs` - Tabela w bazie danych do logowania działań na rezerwacjach
- Automatyczne wypełnianie przez trigger `LogReservationDelete`
- Endpoint API: `GET /api/Reports/reservation-logs` - dostęp do wszystkich logów rezerwacji dla wszystkich użytkowników w systemie (tylko Admin, nie wymaga parametrów)

**Format odpowiedzi dla `/api/Reports/reservation-logs`:**
```json
[
  {
    "id": 1,
    "reservationId": 5,
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "action": "Deleted",
    "logDate": "2024-12-01T10:30:00Z"
  }
]
```
