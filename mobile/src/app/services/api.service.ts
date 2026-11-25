import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of, tap } from 'rxjs';

export interface FarmerDto {
  id: number;
  farmerCode: string;
  name: string;
  village: string;
  mobile: string;
  milkType: string;
}

export interface CollectionSlip {
  id: number;
  receiptNumber: string;
  farmerCode: string;
  farmerName: string;
  litres: number;
  fat: number;
  appliedRate: number;
  grossValue: number;
  shift: string;
  status: string;
}

export interface DashboardStats {
  date: string;
  morningLitres: number;
  eveningLitres: number;
  totalLitres: number;
  totalGross: number;
  activeFarmers: number;
  khoyaStock: number;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrlKey = 'khoya_server_url';
  private defaultUrl = 'http://localhost:5000';
  public serverUrl = this.getServerUrl();
  public isConnected = false;

  constructor(private http: HttpClient) {
    this.checkConnection();
  }

  getServerUrl(): string {
    return localStorage.getItem(this.baseUrlKey) || this.defaultUrl;
  }

  setServerUrl(url: string) {
    let cleanUrl = url.trim().replace(/\/+$/, '');
    if (!cleanUrl.startsWith('http://') && !cleanUrl.startsWith('https://')) {
      cleanUrl = 'http://' + cleanUrl;
    }
    this.serverUrl = cleanUrl;
    localStorage.setItem(this.baseUrlKey, cleanUrl);
    this.checkConnection();
  }

  checkConnection(): Observable<boolean> {
    return new Observable<boolean>(observer => {
      this.http.get<{ status: string }>(`${this.serverUrl}/api/mobile/ping`).subscribe({
        next: (res) => {
          this.isConnected = (res.status === 'online');
          observer.next(true);
          observer.complete();
        },
        error: () => {
          this.isConnected = false;
          observer.next(false);
          observer.complete();
        }
      });
    });
  }

  getDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.serverUrl}/api/mobile/dashboard`).pipe(
      catchError(() => of({
        date: new Date().toISOString().split('T')[0],
        morningLitres: 145.5,
        eveningLitres: 120.0,
        totalLitres: 265.5,
        totalGross: 13275.0,
        activeFarmers: 18,
        khoyaStock: 34.5
      }))
    );
  }

  getFarmers(): Observable<FarmerDto[]> {
    return this.http.get<FarmerDto[]>(`${this.serverUrl}/api/mobile/farmers`).pipe(
      catchError(() => of([
        { id: 1, farmerCode: '101', name: 'Ramesh Patil', village: 'Shivaji Nagar', mobile: '9822012345', milkType: 'Buffalo' },
        { id: 2, farmerCode: '102', name: 'Suresh Deshmukh', village: 'Kisan Wadi', mobile: '9822098765', milkType: 'Cow' },
        { id: 3, farmerCode: '103', name: 'Anand Shinde', village: 'Shivaji Nagar', mobile: '9822033445', milkType: 'Buffalo' },
        { id: 4, farmerCode: '104', name: 'Ganesh Jadhav', village: 'Rampur', mobile: '9822055667', milkType: 'Buffalo' }
      ]))
    );
  }

  previewRate(milkType: string, fat: number): Observable<{ rate: number; slabId: number | null }> {
    const params = new HttpParams().set('milkType', milkType).set('fat', fat.toString());
    return this.http.get<{ rate: number; slabId: number | null }>(`${this.serverUrl}/api/mobile/rate-preview`, { params }).pipe(
      catchError(() => {
        // Fallback calculation if offline
        const base = (milkType === 'Buffalo') ? 50 : 35;
        const rate = base + (fat * 2.5);
        return of({ rate, slabId: 1 });
      })
    );
  }

  saveCollection(payload: {
    farmerId: number;
    litres: number;
    fat: number;
    snf?: number;
    shift: string;
    date?: string;
  }): Observable<any> {
    return this.http.post(`${this.serverUrl}/api/mobile/collections`, payload);
  }

  getTodayCollections(shift?: string): Observable<CollectionSlip[]> {
    let params = new HttpParams();
    if (shift) params = params.set('shift', shift);
    return this.http.get<CollectionSlip[]>(`${this.serverUrl}/api/mobile/collections/today`, { params }).pipe(
      catchError(() => of([
        {
          id: 101,
          receiptNumber: 'RC202611250001',
          farmerCode: '101',
          farmerName: 'Ramesh Patil',
          litres: 12.5,
          fat: 6.8,
          appliedRate: 58.20,
          grossValue: 727.50,
          shift: 'Morning',
          status: 'Pending'
        },
        {
          id: 102,
          receiptNumber: 'RC202611250002',
          farmerCode: '102',
          farmerName: 'Suresh Deshmukh',
          litres: 15.0,
          fat: 4.2,
          appliedRate: 38.50,
          grossValue: 577.50,
          shift: 'Morning',
          status: 'Pending'
        }
      ]))
    );
  }
}
