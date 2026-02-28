import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubscriptionService, SubscriptionDto, UsageDto } from '../proxy/subscription/subscription.service';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-subscription',
  template: `
    <div class="container py-4">
      <div class="row">
        <div class="col-12">
          <h2 class="mb-4">Subscription & Usage</h2>
        </div>
      </div>

      <!-- Current Plan Card -->
      <div class="row mb-4">
        <div class="col-lg-6">
          <div class="card shadow-sm">
            <div class="card-header bg-primary text-white">
              <h4 class="mb-0"><i class="fa fa-credit-card me-2"></i>Current Plan</h4>
            </div>
            <div class="card-body">
              @if (loading) {
                <div class="text-center py-4">
                  <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                  </div>
                </div>
              } @else if (subscription) {
                <div class="mb-3">
                  <h3 class="fw-bold text-capitalize">{{ subscription.plan }}</h3>
                  <span class="badge" [ngClass]="getStatusBadgeClass(subscription.status)">
                    {{ subscription.status }}
                  </span>
                </div>
                @if (subscription.currentPeriodStart && subscription.currentPeriodEnd) {
                  <div class="mb-3">
                    <p class="text-muted mb-1"><strong>Current Period:</strong></p>
                    <p class="mb-0">
                      {{ formatDate(subscription.currentPeriodStart) }} - 
                      {{ formatDate(subscription.currentPeriodEnd) }}
                    </p>
                  </div>
                }
                @if (subscription.cancelAtPeriodEnd) {
                  <div class="alert alert-warning mb-0">
                    <i class="fa fa-exclamation-triangle me-2"></i>
                    Your subscription will cancel at the end of the current period.
                  </div>
                }
                @if (subscription.plan === 'free') {
                  <div class="mt-3">
                    <button class="btn btn-primary" (click)="upgradePlan('pro')">
                      <i class="fa fa-arrow-up me-2"></i>Upgrade to Pro
                    </button>
                  </div>
                }
              } @else {
                <p class="text-muted">No subscription information available.</p>
              }
            </div>
          </div>
        </div>

        <!-- Usage Stats Card -->
        <div class="col-lg-6">
          <div class="card shadow-sm">
            <div class="card-header bg-info text-white">
              <h4 class="mb-0"><i class="fa fa-chart-line me-2"></i>Usage Statistics</h4>
            </div>
            <div class="card-body">
              @if (loading) {
                <div class="text-center py-4">
                  <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                  </div>
                </div>
              } @else if (usage) {
                <!-- Errors Usage -->
                <div class="mb-4">
                  <div class="d-flex justify-content-between mb-2">
                    <span class="fw-bold">Errors Processed</span>
                    <span>{{ usage.errorsProcessed }} / {{ usage.errorsLimit === -1 ? 'Unlimited' : usage.errorsLimit }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div 
                      class="progress-bar" 
                      [ngClass]="getUsageBarClass(usage.errorsProcessed, usage.errorsLimit)"
                      [style.width.%]="getUsagePercentage(usage.errorsProcessed, usage.errorsLimit)"
                      role="progressbar">
                      {{ getUsagePercentage(usage.errorsProcessed, usage.errorsLimit) }}%
                    </div>
                  </div>
                </div>

                <!-- Applications Usage -->
                <div class="mb-4">
                  <div class="d-flex justify-content-between mb-2">
                    <span class="fw-bold">Applications</span>
                    <span>{{ usage.applicationsCount }} / {{ usage.applicationsLimit === -1 ? 'Unlimited' : usage.applicationsLimit }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div 
                      class="progress-bar" 
                      [ngClass]="getUsageBarClass(usage.applicationsCount, usage.applicationsLimit)"
                      [style.width.%]="getUsagePercentage(usage.applicationsCount, usage.applicationsLimit)"
                      role="progressbar">
                      {{ getUsagePercentage(usage.applicationsCount, usage.applicationsLimit) }}%
                    </div>
                  </div>
                </div>

                <!-- Usage Period -->
                <div class="mt-3 pt-3 border-top">
                  <p class="text-muted mb-1"><strong>Usage Period:</strong></p>
                  <p class="mb-0">
                    {{ formatDate(usage.periodStart) }} - {{ formatDate(usage.periodEnd) }}
                  </p>
                </div>
              } @else {
                <p class="text-muted">No usage information available.</p>
              }
            </div>
          </div>
        </div>
      </div>

      <!-- Upgrade Options (for Free users) -->
      @if (subscription && subscription.plan === 'free') {
        <div class="row">
          <div class="col-12">
            <div class="card shadow-sm">
              <div class="card-header">
                <h4 class="mb-0"><i class="fa fa-rocket me-2"></i>Upgrade Your Plan</h4>
              </div>
              <div class="card-body">
                <div class="row g-4">
                  <div class="col-md-6">
                    <div class="card border-primary">
                      <div class="card-body">
                        <h5 class="fw-bold">Pro Plan</h5>
                        <div class="mb-3">
                          <span class="display-6 fw-bold">$49</span>
                          <span class="text-muted">/month</span>
                        </div>
                        <ul class="list-unstyled mb-3">
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>Up to 50,000 errors/month</li>
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>Advanced AI analysis</li>
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>Unlimited applications</li>
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>Priority support</li>
                        </ul>
                        <button class="btn btn-primary w-100" (click)="upgradePlan('pro')">
                          Upgrade to Pro
                        </button>
                      </div>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="card border-secondary">
                      <div class="card-body">
                        <h5 class="fw-bold">Enterprise Plan</h5>
                        <div class="mb-3">
                          <span class="display-6 fw-bold">Custom</span>
                        </div>
                        <ul class="list-unstyled mb-3">
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>Unlimited errors</li>
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>Premium AI features</li>
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>24/7 dedicated support</li>
                          <li class="mb-2"><i class="fa fa-check text-success me-2"></i>SLA guarantee</li>
                        </ul>
                        <button class="btn btn-outline-secondary w-100" (click)="upgradePlan('enterprise')">
                          Contact Sales
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .card {
      transition: transform 0.2s ease;
    }
    .card:hover {
      transform: translateY(-2px);
    }
    .progress-bar {
      transition: width 0.6s ease;
    }
  `],
  imports: [CommonModule]
})
export class SubscriptionComponent implements OnInit {
  private subscriptionService = inject(SubscriptionService);

  subscription: SubscriptionDto | null = null;
  usage: UsageDto | null = null;
  loading = false;
  error: string | null = null;

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;
    this.error = null;

    this.subscriptionService.getCurrent()
      .pipe(
        catchError(err => {
          this.error = 'Failed to load subscription information';
          console.error(err);
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(data => {
        this.subscription = data;
      });

    this.subscriptionService.getUsage()
      .pipe(
        catchError(err => {
          console.error('Failed to load usage information', err);
          return of(null);
        })
      )
      .subscribe(data => {
        this.usage = data;
      });
  }

  upgradePlan(plan: string) {
    this.loading = true;
    this.subscriptionService.createCheckout(plan)
      .pipe(
        catchError(err => {
          this.error = 'Failed to create checkout session';
          console.error(err);
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(result => {
        if (result && result.url) {
          window.location.href = result.url;
        }
      });
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'bg-success';
      case 'trialing':
        return 'bg-info';
      case 'past_due':
        return 'bg-warning';
      case 'canceled':
        return 'bg-secondary';
      default:
        return 'bg-secondary';
    }
  }

  getUsagePercentage(current: number, limit: number): number {
    if (limit === -1) return 0; // Unlimited
    if (limit === 0) return 0;
    const percentage = (current / limit) * 100;
    return Math.min(percentage, 100);
  }

  getUsageBarClass(current: number, limit: number): string {
    const percentage = this.getUsagePercentage(current, limit);
    if (percentage >= 90) return 'bg-danger';
    if (percentage >= 75) return 'bg-warning';
    return 'bg-success';
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
