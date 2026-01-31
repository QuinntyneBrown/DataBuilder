import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Customer } from '../../../models/customer.model';
import { CustomerService } from '../../../services/customer.service';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
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
          <mat-card-title>{{ isNew() ? 'Create' : 'Edit' }} Customer</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          @if (loading()) {
            <div class="loading">
              <mat-spinner diameter="40"></mat-spinner>
            </div>
          } @else {
            <form [formGroup]="form" (ngSubmit)="save()">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Name</mat-label>
                <input matInput formControlName="name">
                @if (form.get('name')?.hasError('required')) {
                  <mat-error>Name is required</mat-error>
                }
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Email</mat-label>
                <input matInput formControlName="email">
                @if (form.get('email')?.hasError('required')) {
                  <mat-error>Email is required</mat-error>
                }
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Age</mat-label>
                <input matInput type="number" formControlName="age">
                @if (form.get('age')?.hasError('required')) {
                  <mat-error>Age is required</mat-error>
                }
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>IsActive</mat-label>
                <input matInput type="checkbox" formControlName="isActive">
                @if (form.get('isActive')?.hasError('required')) {
                  <mat-error>IsActive is required</mat-error>
                }
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>CreatedDate</mat-label>
                <input matInput formControlName="createdDate">
                @if (form.get('createdDate')?.hasError('required')) {
                  <mat-error>CreatedDate is required</mat-error>
                }
              </mat-form-field>

              <div class="actions">
                <button mat-button type="button" routerLink="../">Cancel</button>
                <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || saving()">
                  @if (saving()) {
                    <mat-spinner diameter="20"></mat-spinner>
                  } @else {
                    {{ isNew() ? 'Create' : 'Save' }}
                  }
                </button>
              </div>
            </form>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .container {
      padding: 24px;
      max-width: 600px;
      margin: 0 auto;
    }
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    .loading {
      display: flex;
      justify-content: center;
      padding: 48px;
    }
    .actions {
      display: flex;
      gap: 16px;
      justify-content: flex-end;
      margin-top: 24px;
    }
  `]
})
export class CustomerDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly snackBar = inject(MatSnackBar);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly isNew = signal(true);

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', Validators.required],
    age: ['', Validators.required],
    isActive: ['', Validators.required],
    createdDate: ['', Validators.required],
  });

  private itemId: string | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.itemId = id;
      this.isNew.set(false);
      this.loadItem(id);
    }
  }

  private loadItem(id: string): void {
    this.loading.set(true);
    this.customerService.getById(id).subscribe({
      next: (item) => {
        this.form.patchValue({
          name: item.name,
          email: item.email,
          age: item.age,
          isActive: item.isActive,
          createdDate: item.createdDate,
        });
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load customer', 'Close', { duration: 5000 });
        this.router.navigate(['../'], { relativeTo: this.route });
      }
    });
  }

  save(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    const request = this.form.value;

    const operation = this.isNew()
      ? this.customerService.create(request)
      : this.customerService.update(this.itemId!, request);

    operation.subscribe({
      next: () => {
        this.snackBar.open(
          `Customer ${this.isNew() ? 'created' : 'updated'}`,
          'Close',
          { duration: 3000 }
        );
        this.router.navigate(['../'], { relativeTo: this.route });
      },
      error: () => {
        this.snackBar.open(
          `Failed to ${this.isNew() ? 'create' : 'update'} customer`,
          'Close',
          { duration: 5000 }
        );
        this.saving.set(false);
      }
    });
  }
}
