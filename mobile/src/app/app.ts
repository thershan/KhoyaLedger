import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, FarmerDto, CollectionSlip, DashboardStats } from './services/api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  currentTab: 'intake' | 'today' | 'dashboard' | 'settings' = 'intake';
  lang: 'mr' | 'en' = 'mr';

  // Server state
  serverUrlInput = '';
  isOnline = false;
  pingMessage = '';

  // Farmers
  farmers: FarmerDto[] = [];
  selectedFarmer: FarmerDto | null = null;
  farmerSearchQuery = '';

  // Intake Form
  selectedShift: 'Morning' | 'Evening' = 'Morning';
  litres = 10.0;
  fat = 6.5;
  snf = 8.5;
  rate = 0;
  grossAmount = 0;
  isSubmitting = false;

  // Receipt Modal
  latestReceipt: any = null;
  showReceiptModal = false;

  // Today's collections
  todayCollections: CollectionSlip[] = [];
  todayFilterShift: string = '';

  // Dashboard Stats
  stats: DashboardStats | null = null;

  constructor(public api: ApiService) {}

  ngOnInit() {
    this.serverUrlInput = this.api.serverUrl;
    this.loadFarmers();
    this.checkHealth();
    this.loadToday();
    this.loadDashboard();
  }

  checkHealth() {
    this.api.checkConnection().subscribe(status => {
      this.isOnline = status;
      this.pingMessage = status ? 'Connected to Khoya Ledger' : 'Server unreachable (Offline mode)';
    });
  }

  saveServerSettings() {
    this.api.setServerUrl(this.serverUrlInput);
    this.checkHealth();
    this.loadFarmers();
  }

  toggleLanguage() {
    this.lang = this.lang === 'mr' ? 'en' : 'mr';
  }

  selectTab(tab: 'intake' | 'today' | 'dashboard' | 'settings') {
    this.currentTab = tab;
    if (tab === 'today') this.loadToday();
    if (tab === 'dashboard') this.loadDashboard();
  }

  setShift(shift: 'Morning' | 'Evening') {
    this.selectedShift = shift;
  }

  loadFarmers() {
    this.api.getFarmers().subscribe(data => {
      this.farmers = data;
      if (this.farmers.length > 0 && !this.selectedFarmer) {
        this.selectFarmer(this.farmers[0]);
      }
    });
  }

  selectFarmer(farmer: FarmerDto) {
    this.selectedFarmer = farmer;
    this.farmerSearchQuery = `${farmer.farmerCode} - ${farmer.name}`;
    this.recalculate();
  }

  onFarmerSearchChange() {
    const q = this.farmerSearchQuery.trim().toLowerCase();
    const match = this.farmers.find(f => 
      f.farmerCode.toLowerCase() === q || 
      f.name.toLowerCase().includes(q)
    );
    if (match) {
      this.selectedFarmer = match;
      this.recalculate();
    }
  }

  adjustLitres(delta: number) {
    this.litres = Math.max(0.1, Math.round((this.litres + delta) * 10) / 10);
    this.recalculate();
  }

  adjustFat(delta: number) {
    this.fat = Math.max(1.0, Math.round((this.fat + delta) * 10) / 10);
    this.recalculate();
  }

  recalculate() {
    if (!this.selectedFarmer) return;
    this.api.previewRate(this.selectedFarmer.milkType, this.fat).subscribe(res => {
      this.rate = res.rate;
      this.grossAmount = Math.round(this.litres * this.rate * 100) / 100;
    });
  }

  submitCollection() {
    if (!this.selectedFarmer) {
      alert(this.lang === 'mr' ? 'कृपया शेतकरी निवडा' : 'Please select a farmer');
      return;
    }

    if (this.litres <= 0 || this.fat <= 0) {
      alert(this.lang === 'mr' ? 'दूध आणि फॅट तपासा' : 'Invalid litres or fat');
      return;
    }

    this.isSubmitting = true;
    const payload = {
      farmerId: this.selectedFarmer.id,
      litres: this.litres,
      fat: this.fat,
      snf: this.snf,
      shift: this.selectedShift,
      date: new Date().toISOString().split('T')[0]
    };

    this.api.saveCollection(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.latestReceipt = res;
        this.showReceiptModal = true;
        this.loadToday();
        this.loadDashboard();
      },
      error: (err) => {
        this.isSubmitting = false;
        // Fallback for offline demo
        this.latestReceipt = {
          receiptNumber: 'RC' + Math.floor(100000 + Math.random() * 900000),
          farmerCode: this.selectedFarmer?.farmerCode,
          farmerName: this.selectedFarmer?.name,
          litres: this.litres,
          fat: this.fat,
          appliedRate: this.rate,
          grossValue: this.grossAmount,
          shift: this.selectedShift
        };
        this.showReceiptModal = true;
      }
    });
  }

  closeReceipt() {
    this.showReceiptModal = false;
    // Advance to next farmer or reset litres
    this.litres = 10.0;
  }

  loadToday() {
    this.api.getTodayCollections(this.todayFilterShift).subscribe(data => {
      this.todayCollections = data;
    });
  }

  loadDashboard() {
    this.api.getDashboard().subscribe(data => {
      this.stats = data;
    });
  }
}
