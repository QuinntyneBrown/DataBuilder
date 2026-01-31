import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Customer } from '../../../models/customer.model';
import { CustomerService } from '../../../services/customer.service';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Customers</mat-card-title>
          <span class="spacer"></span>
          <button mat-fab extended color="primary" routerLink="new">
            <mat-icon>add</mat-icon>
            Add Customer
          </button>
        </mat-card-header>
        <mat-card-content>
          @if (loading()) {
            <div class="loading">
              <mat-spinner diameter="40"></mat-spinner>
            </div>
          } @else {
            <table mat-table [dataSource]="items()" class="mat-elevation-z0">
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let item">{{ item.name }}</td>
              </ng-container>
              <ng-container matColumnDef="email">
                <th mat-header-cell *matHeaderCellDef>Email</th>
                <td mat-cell *matCellDef="let item">{{ item.email }}</td>
              </ng-container>
              <ng-container matColumnDef="age">
                <th mat-header-cell *matHeaderCellDef>Age</th>
                <td mat-cell *matCellDef="let item">{{ item.age }}</td>
              </ng-container>
              <ng-container matColumnDef="isActive">
                <th mat-header-cell *matHeaderCellDef>IsActive</th>
                <td mat-cell *matCellDef="let item">{{ item.isActive }}</td>
              </ng-container>
              <ng-container matColumnDef="createdDate">
                <th mat-header-cell *matHeaderCellDef>CreatedDate</th>
                <td mat-cell *matCellDef="let item">{{ item.createdDate }}</td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let item">
                  <button mat-icon-button color="primary" [routerLink]="[item.customerId]">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="delete(item)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }
    mat-card-header {
      display: flex;
      align-items: center;
      margin-bottom: 16px;
    }
    .spacer {
      flex: 1;
    }
    table {
      width: 100%;
    }
    .loading {
      display: flex;
      justify-content: center;
      padding: 48px;
    }
  `]
})
export class CustomerListComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly snackBar = inject(MatSnackBar);

  readonly items = signal<Customer[]>([]);
  readonly loading = signal(true);
  readonly displayedColumns = ['name', 'email', 'age', 'isActive', 'createdDate', 'actions'];

  ngOnInit(): void {
    this.loadItems();
  }

  private loadItems(): void {
    this.loading.set(true);
    this.customerService.getAll().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (error) => {
        this.snackBar.open('Failed to load customers', 'Close', { duration: 5000 });
        this.loading.set(false);
      }
    });
  }

  delete(item: Customer): void {
    if (confirm('Are you sure you want to delete this customer?')) {
      this.customerService.delete(item.customerId).subscribe({
        next: () => {
          this.snackBar.open('Customer deleted', 'Close', { duration: 3000 });
          this.loadItems();
        },
        error: () => {
          this.snackBar.open('Failed to delete customer', 'Close', { duration: 5000 });
        }
      });
    }
  }
}
